using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
                    ProcessTypedef(ast, nativeType, type, context);
                else if (ClangTraits.IsArray(type))
                    ProcessArray(ast, nativeType, type, context);
                // reference and pointer 
                else if (ClangTraits.IsReference(type) || ClangTraits.IsPointer(type))
                    ProcessReferencePointer(ast, nativeType, type, context);
                else if (ClangTraits.IsMemberPointer(type))
                    ProcessMemberPointer(ast, nativeType, cxType, context);
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
                    type.SetFunction(GetFunctionProto(ast, cxType, context));
                }
                else if(ClangTraits.IsUserDefiendType(cxType))
                {
                    NativeClass nativeClass = ast.GetClass(removeQualifierName);

                    // if native class is parsed already, the native class is a full specialization
                    // or the native class is a instantiation of a template or partial specialization
                    if(!nativeClass.IsClassEntity && !nativeClass.Parsed)
                    {
                        nativeClass.Parsed = true;
                        if(TemplateHelper.VisitTemplate(cursor, nativeClass, ast))
                        {
                            TemplateHelper.VisitTemplateParameter(cursor, theType, nativeClass, ast, context);
                        }
                    }

                    type.SetClass(nativeClass);
                }
            }
        }

        private static void ProcessTypedef(AST ast, NativeType type, CXType cxType, TypeVisitContext context)
        {
            // get type redirection
            CXCursor typedefedCursor = clang.getTypeDeclaration(cxType);

            CXCursor refCursor = clang.getCursorReferenced(typedefedCursor);

            CXType typedefedType = clang.getTypedefDeclUnderlyingType(typedefedCursor);
            NativeType typedefedNativeType = GetNativeType(ast, typedefedType, context);
            type.SetTypedef(typedefedNativeType);
        }

        private static void ProcessArray(AST ast, NativeType type, CXType cxType, TypeVisitContext context)
        {
            // set as array
            CXType elementType = clang.getArrayElementType(cxType);
            NativeArrayType arr = new NativeArrayType
            {
                Count = (int)clang.getArraySize(cxType),
                Type = GetNativeType(ast, elementType, context)
            };
            type.SetArray(arr);
        }

        private static void ProcessReferencePointer(AST ast, NativeType type, CXType cxType, TypeVisitContext context)
        {
            Debug.Assert(type.TypeKind == BasicType.Unknown);
            type.IsConst = ClangTraits.IsConst(cxType);

            CXType pointeeType = clang.getPointeeType(cxType);
            NativeType nativeType = GetNativeType(ast, pointeeType, context);

            if (ClangTraits.IsLValueReference(cxType))
                type.SetTypeLValRef(nativeType);
            else if (ClangTraits.IsRValueReference(cxType))
                type.SetTypeRValRef(nativeType);
            else if (ClangTraits.IsPointer(cxType))
                type.SetPointer(nativeType);

            Debug.Assert(type.TypeKind != BasicType.Unknown);
        }

        private static void ProcessMemberPointer(AST ast, NativeType type, CXType cxType, TypeVisitContext context)
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
                    Function = GetFunctionProto(ast, pointeeType, context)
                });
            }
            else
            {
                type.SetPMD(new MemberDataPointer
                {
                    Class = nativeClass,
                    Data = GetNativeType(ast, pointeeType, context)
                });
            }
        }

        private static FunctionProto GetFunctionProto(AST ast, CXType funcType, TypeVisitContext context)
        {
            Debug.Assert(ClangTraits.IsFunction(funcType));
            FunctionProto proto = new FunctionProto();
            proto.ResultType = GetNativeType(ast, clang.getResultType(funcType), context);
            uint arity = (uint)clang.getNumArgTypes(funcType);
            for(uint loop = 0; loop < arity; ++loop)
            {
                CXType argType = clang.getArgType(funcType, loop);
                FunctionParameter param = new FunctionParameter();
                param.Type = GetNativeType(ast, argType, context);
                proto.AddParameter(param);
            }

            return proto;
        }
    }

    public class TypeVisitContextItem
    {
        public CXCursorKind Kind { get; set; }
        public string CtxString { get; set; }
    }

    public class TypeVisitContext
    {
        private List<string> context_;

        public TypeVisitContext(CXCursor cursor)
        {
            List<string> context = new List<string>();
            GCHandle contextHandle = GCHandle.Alloc(context);
            clang.visitChildren(cursor, ContextVisitor, new CXClientData((IntPtr)contextHandle));
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

        public bool Empty { get { return Count <= 0; } }

        private CXChildVisitResult ContextVisitor(CXCursor cursor, CXCursor parent, IntPtr data)
        {
            // prepare context handle
            GCHandle contextHandle = (GCHandle)data;
            List<string> context = contextHandle.Target as List<string>;

            if (ClangTraits.IsNonTypeTemplateParamLiteral(cursor))
            {
                List<string> tokens = ASTVisitor.GetCursorTokens(cursor);
                string literal = string.Concat(tokens);
                context.Add(literal);
            }
            else if(ClangTraits.IsTemplateRef(cursor))
            {
                CXCursor refCursor = clang.getCursorReferenced(cursor);
                if (ClangTraits.IsTemplateAlias(refCursor))
                {
                    //clang.visitChildren(refCursor, ContextVisitor, new CXClientData(data));
                    clang.visitChildren(refCursor, (CXCursor c, CXCursor p, IntPtr d) =>
                    {
                        if (CXCursorKind.CXCursor_TypeAliasDecl == c.kind)
                            return CXChildVisitResult.CXChildVisit_Recurse;
                        ContextVisitor(c, p, d);
                        return CXChildVisitResult.CXChildVisit_Continue;
                    }, new CXClientData(data));
                }
            }

            return CXChildVisitResult.CXChildVisit_Continue;
        }
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
            NativeType type = TypeVisitorHelper.GetNativeType(AST_, cxType, context);
            return true;
        }
    }

}
