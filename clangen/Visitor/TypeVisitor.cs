using ClangSharp;
using System.Diagnostics;

namespace clangen
{
    public class TypeVisitor
    {
        public static NativeType GetNativeType(AST ast, CXType type)
        {
            string typeName = clang.getTypeSpelling(type).ToString();
            NativeType nativeType = ast.GetType(typeName);

            if(!nativeType.Parsed)
            {
                // not a type reference nor a type with qualifiers
                if (ClangTraits.IsTypeEntity(type))
                    ProcessTypeEntity(ast, nativeType, type);
                // using or typedef
                else if (ClangTraits.IsTypedef(type))
                    ProcessTypedef(ast, nativeType, type);
                // reference and pointer 
                else
                    ProcessQualifiers(ast, nativeType, type);
                nativeType.Parsed = true;
            }

            return nativeType;
        }

        private static void ProcessTypeEntity(AST ast, NativeType type, CXType cxType)
        {
            type.IsConst = ClangTraits.IsConst(cxType);
            if (ClangTraits.IsBuiltInType(cxType))
            {
                type.Info.SetBasicType(ClangTraits.ToBasicType(cxType));
            }
            else
            {
                CXCursor cursor = clang.getTypeDeclaration(cxType);
                CXType theType = clang.getCursorType(cursor);
                string removeQualifierName = clang.getTypeSpelling(theType).ToString();

                // TODO ... dealing with template instantiation
                //if(ClangTraits.IsUnexposedType(tmp))
                //{
                //    CXCursor ccc = clang.getSpecializedCursorTemplate(cursor);
                //}

                if (ClangTraits.IsEnum(cxType))
                {
                    type.Info.SetEnum(ast.GetEnum(removeQualifierName));
                }
                else
                {
                    type.Info.SetClass(ast.GetClass(removeQualifierName));
                }
            }
        }

        private static void ProcessTypedef(AST ast, NativeType type, CXType cxType)
        {
            // get type redirection
            CXCursor typedefedCursor = clang.getTypeDeclaration(cxType);
            CXType typedefedType = clang.getTypedefDeclUnderlyingType(typedefedCursor);
            // TODO ... dealing with tempalte instantiation
            //if (ClangTraits.IsUnexposedType(typedefedType))
            //{
            //    CXCursor instanceCursor = clang.getTypeDeclaration(typedefedType);
            //    CXCursor templateCursor = clang.getSpecializedCursorTemplate(instanceCursor);
            //
            //}
            NativeType typedefedNativeType = GetNativeType(ast, typedefedType);
            
            type.SetReferencedType(typedefedNativeType);
        }

        private static void ProcessQualifiers(AST ast, NativeType type, CXType cxType)
        {
            Debug.Assert(type.Qualifier == QulifierType.Unknown);
            type.IsConst = ClangTraits.IsConst(cxType);
            if (ClangTraits.IsLValueReference(cxType))
            {
                type.Qualifier = QulifierType.LRef;
            }
            else if (ClangTraits.IsRValueReference(cxType))
            {
                type.Qualifier = QulifierType.RRef;
            }
            else if (ClangTraits.IsPointer(cxType))
            {
                type.Qualifier = QulifierType.Ptr;
            }
            Debug.Assert(type.Qualifier != QulifierType.Unknown);

            CXType pointeeType = clang.getPointeeType(cxType);
            NativeType nativeType = GetNativeType(ast, pointeeType);
            type.SetReferencedType(nativeType);
        }
    }
}
