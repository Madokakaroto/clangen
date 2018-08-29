using ClangSharp;
using System.Diagnostics;

namespace clangen
{
    public class TypeVisitHelper
    {
        static NativeType GetNativeType(AST ast, CXType type)
        {
            string typeName = clang.getTypeSpelling(type).ToString();

            NativeType nativeType = ast.GetType(typeName);
            if(!nativeType.Parsed)
            {
                CXType tmp = type;
                while (!ClangTraits.IsInvalid(tmp))
                {
                    bool isConst = ClangTraits.IsConst(tmp);

                    if(ClangTraits.IsTypeEntity(tmp))
                    {
                        nativeType.IsConst = isConst;
                        if(ClangTraits.IsBuiltInType(tmp))
                        {
                            nativeType.Info.SetBasicType(ClangTraits.ToBasicType(tmp));
                        }
                        else
                        {
                            CXCursor cursor = clang.getTypeDeclaration(tmp);
                            CXType theType = clang.getCursorType(cursor);
                            string removeQualifierName = clang.getTypeSpelling(theType).ToString();

                            //if(ClangTraits.IsUnexposedType(tmp))
                            //{
                            //    CXCursor ccc = clang.getSpecializedCursorTemplate(cursor);
                            //}

                            if (ClangTraits.IsEnum(tmp))
                            {
                                nativeType.Info.SetEnum(ast.GetEnum(removeQualifierName));
                            }
                            else
                            {
                                nativeType.Info.SetClass(ast.GetClass(removeQualifierName));
                            }
                        }
                    }
                    else if(ClangTraits.IsTypedef(tmp))
                    {
                        // get type redirection
                        CXCursor typedefedCursor = clang.getTypeDeclaration(tmp);
                        CXType typedefedType = clang.getTypedefDeclUnderlyingType(typedefedCursor);

                        // dealing with tempalte instantiation
                        if (ClangTraits.IsUnexposedType(typedefedType))
                        {
                            CXCursor instanceCursor = clang.getTypeDeclaration(typedefedType);
                            CXCursor templateCursor = clang.getSpecializedCursorTemplate(instanceCursor);

                        }
                        NativeType typedefedNativeType = GetNativeType(ast, typedefedType);
                        
                        nativeType.IsConst = isConst;
                        nativeType.SetTypedefedType(typedefedNativeType);
                    }
                    else
                    {
                        //if (ClangTraits.IsLValueReference(tmp))
                        //{
                        //    nativeType.Qualifiers.PushLValueReference();
                        //}
                        //else if (ClangTraits.IsRValueReference(tmp))
                        //{
                        //    nativeType.Qualifiers.PushRValueReference();
                        //}
                        //else if (ClangTraits.IsPointer(tmp))
                        //{
                        //    if (isConst)
                        //        nativeType.Qualifiers.PushConstPointer();
                        //    else
                        //        nativeType.Qualifiers.PushPointer();
                        //}
                    }

                    tmp = clang.getPointeeType(tmp);
                }

                nativeType.Parsed = true;
            }
            return nativeType;
        }
    }

    public class TypeVisitor
    {
        public static NativeType GetNativeType(AST ast, CXType type)
        {
            string typeName = clang.getTypeSpelling(type).ToString();
            NativeType nativeType = ast.GetType(typeName);

            if(!nativeType.Parsed)
            {
                CXType tmp = type;
                while (!ClangTraits.IsInvalid(tmp))
                {
                    
                    if (ClangTraits.IsTypeEntity(tmp))         // not a type reference nor a type with qualifiers
                        ProcessTypeEntity(ast, nativeType, tmp);
                    else if (ClangTraits.IsTypedef(type))       // using or typedef
                        ProcessTypedef(ast, nativeType, tmp);
                    else                                        // reference and pointer 
                        ProcessQualifiers(nativeType, tmp);
                    tmp = clang.getPointeeType(tmp);
                }
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
            
            type.SetTypedefedType(typedefedNativeType);
        }

        private static void ProcessQualifiers(NativeType type, CXType cxType)
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
        }
    }
}
