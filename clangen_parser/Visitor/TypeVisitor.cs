using System;
using System.Diagnostics;
using System.Collections.Generic;
using ClangSharp;

namespace clangen
{
    public class TypeVisitorHelper
    {
        public static NativeType GetNativeType(AST ast, CXType cxType, TypeVisitContext context = null)
        {
            CXType type = cxType;
            if(ClangTraits.IsElaboratedType(type) || ClangTraits.IsUnexposedType(type))
            {
                type = clang.getCursorType(clang.getTypeDeclaration(type));
            }
            Debug.Assert(!ClangTraits.IsInvalid(type));

            string typeName = clang.getTypeSpelling(type).ToString();
            Debug.Assert(typeName.Length > 0);
            NativeType nativeType = ast.GetType(typeName);

            if(!nativeType.Parsed)
            {
                nativeType.Parsed = true;

                // get cursor spelling as unscoped name
                CXCursor declaration = clang.getTypeDeclaration(type);
                nativeType.UnscopedName = clang.getCursorSpelling(declaration).ToString();

                // not a type reference nor a type with qualifiers
                if (ClangTraits.IsTypeEntity(type) || typeName == "std::nullptr_t")
                    ProcessTypeEntity(ast, nativeType, type, context);
                // using or typedef
                else if (ClangTraits.IsTypedef(type))
                    ProcessTypedef(ast, nativeType, type);
                else if (ClangTraits.IsArray(type))
                    ProcessArray(ast, nativeType, type);
                // reference and pointer 
                else if (ClangTraits.IsReference(type) || ClangTraits.IsPointer(type))
                    ProcessReferencePointer(ast, nativeType, type);
                else if (ClangTraits.IsMemberPointer(type))
                    ProcessMemberPointer(ast, nativeType, cxType);
                else
                    Debug.Assert(false);
            }

            return nativeType;
        }

        private static void ProcessTypeEntity(
            AST ast, 
            NativeType type, 
            CXType cxType,
            TypeVisitContext context)
        {
            type.IsConst = ClangTraits.IsConst(cxType);
            if (ClangTraits.IsBuiltInType(cxType))
            {
                type.SetBuiltin(ClangTraits.ToBasicType(cxType));
            }
            else
            {
                CXCursor cursor = clang.getTypeDeclaration(cxType);
                CXType theType = clang.getCursorType(cursor);
                string removeQualifierName = clang.getTypeSpelling(theType).ToString();

                if (ClangTraits.IsEnum(cxType))
                {
                    type.SetEnum(ast.GetEnum(removeQualifierName));
                }
                else if(ClangTraits.IsFunction(cxType))
                {
                    type.SetFunction(GetFunctionProto(ast, cxType));
                }
                else if(ClangTraits.IsUserDefiendType(cxType))
                {
                    NativeClass nativeClass = ast.GetClass(removeQualifierName);

                    // if native class is parsed already, the native class is a full specialization
                    // or the native class is a instantiation of a template or partial specialization
                    if(!nativeClass.Parsed)
                    {
                        nativeClass.Parsed = true;
                        if(TemplateHelper.VisitTemplate(cursor, nativeClass, ast))
                        {
                            if (context != null) context.Consume();
                            TemplateHelper.VisitTemplateParameter(cursor, theType, nativeClass, ast, context);
                        }
                    }

                    type.SetClass(nativeClass);
                }
            }
        }

        private static void ProcessTypedef(AST ast, NativeType type, CXType cxType)
        {
            // get type redirection
            CXCursor typedefedCursor = clang.getTypeDeclaration(cxType);
            CXType typedefedType = clang.getTypedefDeclUnderlyingType(typedefedCursor);
            NativeType typedefedNativeType = GetNativeType(ast, typedefedType);
            type.SetTypedef(typedefedNativeType);
        }

        private static void ProcessArray(AST ast, NativeType type, CXType cxType)
        {
            // set as array
            CXType elementType = clang.getArrayElementType(cxType);
            NativeArrayType arr = new NativeArrayType
            {
                Count = (int)clang.getArraySize(cxType),
                Type = GetNativeType(ast, elementType)
            };
            type.SetArray(arr);
        }

        private static void ProcessReferencePointer(AST ast, NativeType type, CXType cxType)
        {
            Debug.Assert(type.TypeKind == BasicType.Unknown);
            type.IsConst = ClangTraits.IsConst(cxType);

            CXType pointeeType = clang.getPointeeType(cxType);
            NativeType nativeType = GetNativeType(ast, pointeeType);

            if (ClangTraits.IsLValueReference(cxType))
                type.SetTypeLValRef(nativeType);
            else if (ClangTraits.IsRValueReference(cxType))
                type.SetTypeRValRef(nativeType);
            else if (ClangTraits.IsPointer(cxType))
                type.SetPointer(nativeType);

            Debug.Assert(type.TypeKind != BasicType.Unknown);
        }

        private static void ProcessMemberPointer(AST ast, NativeType type, CXType cxType)
        {
            CXType classType = clang.Type_getClassType(cxType);
            string className = clang.getTypeSpelling(classType).ToString();
            NativeClass nativeClass = ast.GetClass(className);

            CXType pointeeType = clang.getPointeeType(cxType);
            if (ClangTraits.IsFunction(pointeeType))
            {
                type.SetPMF(new MemberFunctionPointer
                {
                    Class = nativeClass,
                    Function = GetFunctionProto(ast, pointeeType)
                });
            }
            else
            {
                type.SetPMD(new MemberDataPointer
                {
                    Class = nativeClass,
                    Data = GetNativeType(ast, pointeeType)
                });
            }
        }

        private static FunctionProto GetFunctionProto(AST ast, CXType funcType)
        {
            Debug.Assert(ClangTraits.IsFunction(funcType));
            FunctionProto proto = new FunctionProto();
            proto.ResultType = GetNativeType(ast, clang.getResultType(funcType));
            uint arity = (uint)clang.getNumArgTypes(funcType);
            for(uint loop = 0; loop < arity; ++loop)
            {
                CXType argType = clang.getArgType(funcType, loop);
                FunctionParameter param = new FunctionParameter();
                param.Type = GetNativeType(ast, argType);
                proto.AddParameter(param);
            }

            return proto;
        }
    }

    public class TypeVisitContext
    {
        private List<string> context_;

        public TypeVisitContext(CXCursor cursor)
        {
            List<string> context = new List<string>();
            clang.visitChildren(cursor, (CXCursor c, CXCursor p, IntPtr data) =>
            {
                if (ClangTraits.IsNonTypeTemplateParamLiteral(c))
                {
                    List<string> tokens = ASTVisitor.GetCursorTokens(c);
                    context.Add(string.Concat(tokens));
                }
                else if (ClangTraits.IsTemplateRef(c))
                {
                    CXCursor refCursor = clang.getCursorReferenced(c);
                    string templateID = clang.getCursorUSR(refCursor).ToString();
                    context.Add(templateID);
                }
                return CXChildVisitResult.CXChildVisit_Continue;
            }, new CXClientData(IntPtr.Zero));
            context_ = context;
        }

        public string Consume()
        {
            Debug.Assert(context_.Count > 0);
            string top = context_[0];
            context_.RemoveAt(0);
            return top;
        }

        public int Count { get { return context_.Count; } }
    }

    public class TypeVisitor : IASTVisitor
    {
        AST AST_;

        public TypeVisitor(AST ast)
        {
            AST_ = ast;
        }

        public bool DoVisit(CXCursor cursor, CXCursor parent)
        {
            CXType cxType = clang.getCursorType(cursor);
            TypeVisitContext context = new TypeVisitContext(cursor);
            context.Consume();
            NativeType type = TypeVisitorHelper.GetNativeType(AST_, cxType, context);
            Debug.Assert(ASTTraits.IsTypedef(type));
            return true;
            //NativeType typedefedType = type.Type as NativeType;
            //if (ASTTraits.IsObject(typedefedType.TypeKind))
            //{
            //    NativeClass @class = typedefedType.Type as NativeClass;
            //    if (!@class.Parsed)
            //    {
            //        @class.Parsed = true;
            //        
            //
            //        TemplateHelper.VisitTemplateParameter(cursor, cxType, @class, AST_);
            //    }
            //}
        }
    }

}
