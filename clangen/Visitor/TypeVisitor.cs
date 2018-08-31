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

            string typeName = clang.getTypeSpelling(type).ToString();
            NativeType nativeType = ast.GetType(typeName);

            if(!nativeType.Parsed)
            {
                // not a type reference nor a type with qualifiers
                if (ClangTraits.IsTypeEntity(type))
                    ProcessTypeEntity(ast, nativeType, type, ClangTraits.IsUnexposedType(cxType));
                // using or typedef
                else if (ClangTraits.IsTypedef(type))
                    ProcessTypedef(ast, nativeType, type);
                else if (ClangTraits.IsArray(type))
                    ProcessArray(ast, nativeType, type);
                // reference and pointer 
                else
                    ProcessQualifiers(ast, nativeType, type);
                nativeType.Parsed = true;
            }

            return nativeType;
        }

        private static void ProcessTypeEntity(AST ast, NativeType type, CXType cxType, bool isInstanciation)
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

                if (ClangTraits.IsEnum(cxType))
                {
                    type.Info.SetEnum(ast.GetEnum(removeQualifierName));
                }
                else if(ClangTraits.IsUserDefiendType(cxType))
                {
                    NativeClass nativeClass = ast.GetClass(removeQualifierName);

                    // if native class is parsed already, the native class is a full specialization
                    // or the native class is a instantiation of a template or partial specialization
                    if(isInstanciation && !nativeClass.Parsed)
                    {
                        CXCursor templateCurosr = clang.getSpecializedCursorTemplate(cursor);
                        string templateID = clang.getCursorUSR(templateCurosr).ToString();
                        ClassTemplate template = ast.GetClassTemplate(templateID);
                        Debug.Assert(template.Parsed);
                        
                        // TODO ... template instantiation

                        nativeClass.Parsed = true;
                    }

                    type.Info.SetClass(nativeClass);
                }
            }
        }

        private static void ProcessTypedef(AST ast, NativeType type, CXType cxType)
        {
            // get type redirection
            CXCursor typedefedCursor = clang.getTypeDeclaration(cxType);
            CXType typedefedType = clang.getTypedefDeclUnderlyingType(typedefedCursor);
            NativeType typedefedNativeType = GetNativeType(ast, typedefedType);
            type.SetReferencedType(typedefedNativeType);
        }

        private static void ProcessArray(AST ast, NativeType type, CXType cxType)
        {
            // set as array
            type.IsArray = true;
            if (!ClangTraits.IsIncompleteArray(cxType))
            {
                type.Count = (int)clang.getArraySize(cxType);
            }

            // get element type
            CXType elementType = clang.getArrayElementType(cxType);
            NativeType nativeType = GetNativeType(ast, elementType);
            type.SetReferencedType(nativeType);
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
            NativeType nativeType = GetNativeType(ast, pointeeType);
            type.SetReferencedType(nativeType);
        }
    }

    public class DependentTypeVisitor
    {
        //public static void 
    }
}
