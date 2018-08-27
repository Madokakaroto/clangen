using ClangSharp;

namespace clangen
{
    public class TypeVisitHelper
    {
        static public NativeType GetNativeType(AST ast, CXType type)
        {
            string typeName = clang.getTypeSpelling(type).ToString();
            bool unsetteld = false;
            NativeType nativeType = ast.GetType(typeName, out unsetteld);
            if(unsetteld)
            {
                CXType tmp = type;
                while (!ClangTraits.IsInvalid(tmp))
                {
                    bool isConst = ClangTraits.IsConst(tmp);

                    if(ClangTraits.IsTerminalType(tmp))
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
                            typedefedCursor = clang.getTypeDeclaration(typedefedType);
                            typedefedType = clang.getCursorType(typedefedCursor);
                        }
                        NativeType typedefedNativeType = GetNativeType(ast, typedefedType);
                        
                        nativeType.IsConst = isConst;
                        nativeType.SetTypedefedType(typedefedNativeType);
                    }
                    else
                    {
                        if (ClangTraits.IsLValueReference(tmp))
                        {
                            nativeType.Qualifiers.PushLValueReference();
                        }
                        else if (ClangTraits.IsRValueReference(tmp))
                        {
                            nativeType.Qualifiers.PushRValueReference();
                        }
                        else if (ClangTraits.IsPointer(tmp))
                        {
                            if (isConst)
                                nativeType.Qualifiers.PushConstPointer();
                            else
                                nativeType.Qualifiers.PushPointer();
                        }
                    }

                    tmp = clang.getPointeeType(tmp);
                }
            }
            return nativeType;
        }

    }
}
