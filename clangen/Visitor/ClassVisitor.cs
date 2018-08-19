using System;
using System.Runtime.InteropServices;
using ClangSharp;

namespace clangen
{
    class ClassVisitor : IASTVisitor
    {
        private AST AST_;

        public ClassVisitor(AST ast)
        {
            AST_ = ast;
        }

        public bool DoVisit(CXCursor cursor, CXCursor parent)
        {
            // get class token name
            string className = clang.getCursorSpelling(cursor).ToString();

            // get class object
            bool unsetteld = false;
            Class @class = AST_.GetClass(className, out unsetteld);
            if (unsetteld)
            {
                // create IntPtr for context
                GCHandle astHandle = GCHandle.Alloc(@class);

                // visit children
                clang.visitChildren(cursor, Visitor, new CXClientData((IntPtr)astHandle));

                // add class
                AST_.AddClass(@class);
            }

            return true;
        }

        private CXChildVisitResult Visitor(CXCursor cursor, CXCursor parent, IntPtr data)
        {
            CXCursorKind kind = clang.getCursorKind(cursor);
            switch(kind)
            {
                case CXCursorKind.CXCursor_CXXMethod:
                    break;
                case CXCursorKind.CXCursor_FieldDecl:
                    break;
                case CXCursorKind.CXCursor_CXXBaseSpecifier:
                    // remove class type and many many...
                    CXCursor refCursor = clang.getCursorReferenced(cursor);
                    string name = clang.getCursorSpelling(refCursor).ToString();
                    break;
                default:
                    break;
            }

            return CXChildVisitResult.CXChildVisit_Continue;
        }
    }
}
