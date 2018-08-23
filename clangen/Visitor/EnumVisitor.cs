using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ClangSharp;

namespace clangen
{
    class EnumVisitor : IASTVisitor
    {
        private AST AST_;

        public EnumVisitor(AST ast)
        {
            AST_ = ast;
        }

        public bool DoVisit(CXCursor cursor, CXCursor parent)
        {
            string name = clang.getCursorSpelling(cursor).ToString();

            bool unsettled = false;
            Enumeration @enum = AST_.GetEnum(name, out unsettled);
            if(unsettled)
            {
                // is scoped
                @enum.IsEnumClass = clang.EnumDecl_isScoped(cursor) != 0;

                // create IntPtr for context
                GCHandle astHandle = GCHandle.Alloc(@enum);

                 // visit children
                clang.visitChildren(cursor, Visitor, new CXClientData((IntPtr)astHandle));

                // add class
                AST_.AddEnum(@enum);
            }

            return true;
        }

        private CXChildVisitResult Visitor(CXCursor cursor, CXCursor parent, IntPtr data)
        {
            if(cursor.kind == CXCursorKind.CXCursor_EnumConstantDecl)
            {
                // prepare enumeration
                  // prepare client data
                GCHandle astHandle = (GCHandle)data;
                Enumeration thisEnum = astHandle.Target as Enumeration;

                // get constant name
                string constantName = clang.getCursorSpelling(cursor).ToString();

                // get underlying type
                CXType underlyingType = clang.getEnumDeclIntegerType(parent);
                BasicType type = ClangTraits.ToBasicType(underlyingType);

                // get enum constant value
                EnumConstant c;
                if(ClangTraits.IsSigned(underlyingType))
                {
                    long constantValue = clang.getEnumConstantDeclValue(cursor);
                    c = EnumConstant.Create(constantName, type, constantValue);
                }
                else
                {
                    Debug.Assert(ClangTraits.IsUnsigned(underlyingType));
                    ulong constantValue = clang.getEnumConstantDeclUnsignedValue(cursor);
                    c = EnumConstant.Create(constantName, type, constantValue);
                }

                // add it to enumeration
                thisEnum.AddConstant(c);
            }
            return CXChildVisitResult.CXChildVisit_Continue;
        }
    }
}
