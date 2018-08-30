using ClangSharp;
using System;
using System.Diagnostics;

namespace clangen
{
    public class TypeVisitor
    {
        public static NativeType GetNativeType(AST ast, CXType cxType, bool isDefinition)
        {
            CXType type = cxType;
            if(ClangTraits.IsElaboratedType(type) || ClangTraits.IsUnexposedType(type))
            {
                type = clang.getCursorType(clang.getTypeDeclaration(type));
            }

            string typeName = clang.getTypeSpelling(type).ToString();
            NativeType nativeType = ast.GetType(typeName);

            if(!nativeType.Parsed && isDefinition)
            {
                // not a type reference nor a type with qualifiers
                if (ClangTraits.IsTypeEntity(type))
                    ProcessTypeEntity(ast, nativeType, type, ClangTraits.IsUnexposedType(cxType));
                // using or typedef
                else if (ClangTraits.IsTypedef(type))
                    ProcessTypedef(ast, nativeType, type, isDefinition);
                // reference and pointer 
                else
                    ProcessQualifiers(ast, nativeType, type, isDefinition);
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

                        clang.Type_visitFields(theType, (CXCursor c, IntPtr data)=>
                        {
                            CXCursorKind kind = c.kind;
                            return CXVisitorResult.CXVisit_Continue;
                        }, new CXClientData(IntPtr.Zero));

                        // TODO ...
                        nativeClass.Parsed = true;
                    }

                    type.Info.SetClass(nativeClass);
                }
            }
        }

        private static void ProcessTypedef(AST ast, NativeType type, CXType cxType, bool isDefinition)
        {
            // get type redirection
            CXCursor typedefedCursor = clang.getTypeDeclaration(cxType);
            CXType typedefedType = clang.getTypedefDeclUnderlyingType(typedefedCursor);
            NativeType typedefedNativeType = GetNativeType(ast, typedefedType, isDefinition);
            type.SetReferencedType(typedefedNativeType);
        }

        private static void ProcessQualifiers(AST ast, NativeType type, CXType cxType, bool isDefinition)
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
            NativeType nativeType = GetNativeType(ast, pointeeType, isDefinition);
            type.SetReferencedType(nativeType);
        }
    }

    public class DependentTypeVisitor
    {
        //public static void 
    }
}
