using System;
using System.Collections.Generic;
using System.Text;
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
                        nativeType.Type = ClangTraits.ToBasicType(tmp);

                        if (ClangTraits.IsUserDefiendType(tmp))
                        {
                            CXCursor cursor = clang.getTypeDeclaration(tmp);
                            CXType theType = clang.getCursorType(cursor);
                            string removeQualifierName = clang.getTypeSpelling(theType).ToString();
                            nativeType.Class = ast.GetClass(removeQualifierName);
                        }
                    }
                    else if(ClangTraits.IsTypedef(tmp))
                    {
                        CXCursor typedefedCursor = clang.getTypeDeclaration(tmp);
                        CXType typedefedType = clang.getTypedefDeclUnderlyingType(typedefedCursor);
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
