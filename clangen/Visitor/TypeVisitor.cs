using ClangSharp;
using System.Diagnostics;

namespace clangen
{
    public class TypeVisitor
    {
        public static NativeType GetNativeType(AST ast, CXType type, bool isDefinition)
        {
            string typeName = clang.getTypeSpelling(type).ToString();
            NativeType nativeType = ast.GetType(typeName);

            if(!nativeType.Parsed && isDefinition)
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

                //if(ClangTraits.IsUnexposedType(cxType))
                //{
                //    CXCursor ccc = clang.getSpecializedCursorTemplate(cursor);
                //}

                if (ClangTraits.IsEnum(cxType))
                {
                    type.Info.SetEnum(ast.GetEnum(removeQualifierName));
                }
                else if(ClangTraits.IsUserDefiendType(cxType))
                {
                    type.Info.SetClass(ast.GetClass(removeQualifierName));
                }
                else if(ClangTraits.IsUnexposedType(cxType))
                {
                    CXCursor ccc = clang.getSpecializedCursorTemplate(cursor);
                }
            }
        }

        private static void ProcessTypedef(AST ast, NativeType type, CXType cxType)
        {
            // get type redirection
            CXCursor typedefedCursor = clang.getTypeDeclaration(cxType);
            CXType typedefedType = clang.getTypedefDeclUnderlyingType(typedefedCursor);
            
            // dealing with elaborated type
            if(ClangTraits.IsElaboratedType(typedefedType) || ClangTraits.IsUnexposedType(typedefedType))
            {
                typedefedType = clang.getCursorType(clang.getTypeDeclaration(typedefedType));
            }

            // typedef and using dose not trigger template instantiation
            NativeType typedefedNativeType = GetNativeType(ast, typedefedType, false);

            // dealing with tempalte instantiation
            //if (ClangTraits.IsUnexposedType(typedefedType))
            //{
            //    CXCursor instanceCursor = clang.getTypeDeclaration(typedefedType);
            //    CXCursor templateCursor = clang.getSpecializedCursorTemplate(instanceCursor);
            //
            //    string templateID = clang.getCursorUSR(templateCursor).ToString();
            //    ClassTemplate template = ast.GetClassTemplate(templateID);
            //
            //    // TODO ...set template information
            //}

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
            // pointer and reference type requires only forward declaration and 
            // does not trigger template instantiation
            NativeType nativeType = GetNativeType(ast, pointeeType, false);
            type.SetReferencedType(nativeType);
        }
    }

    public class DependentTypeVisitor
    {
        //public static void 
    }
}
