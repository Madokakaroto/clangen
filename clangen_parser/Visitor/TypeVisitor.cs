using ClangSharp;
using System;
using System.Diagnostics;

namespace clangen
{
    public class TypeVisitor
    {
        public static NativeType GetNativeType(AST ast, CXType cxType)
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
                    ProcessTypeEntity(ast, nativeType, type, ClangTraits.IsUnexposedType(cxType));
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

        private static void ProcessTypeEntity(AST ast, NativeType type, CXType cxType, bool isInstanciation)
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
                    if(isInstanciation && !nativeClass.Parsed)
                    {
                        bool result = TemplateHelper.VisitTemplateArguments(cursor, nativeClass, ast);
                        Debug.Assert(result);
                        nativeClass.Parsed = result;
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

    public class DependentTypeVisitor
    {
        //public static void 
    }
}
