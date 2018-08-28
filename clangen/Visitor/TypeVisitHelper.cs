using ClangSharp;
using System;

namespace clangen
{
    public class TypeVisitHelper
    {
        static public NativeType GetNativeType(AST ast, CXType type)
        {
            string typeName = clang.getTypeSpelling(type).ToString();

            NativeType nativeType = ast.GetType(typeName);
            if(!nativeType.Parsed)
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

                nativeType.Parsed = true;
            }
            return nativeType;
        }

    }
}
