using System;
using System.Diagnostics;
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

            // get cursor kind
            CXCursorKind cursorKind = clang.getCursorKind(cursor);
            Debug.Assert(
                cursorKind == CXCursorKind.CXCursor_ClassDecl ||
                cursorKind == CXCursorKind.CXCursor_StructDecl);

            // get class object
            bool unsetteld = false;
            StructOrClass classTag = cursorKind == CXCursorKind.CXCursor_StructDecl ? StructOrClass.Struct : StructOrClass.Class;
            NativeClass @class = AST_.GetClass(className, classTag, out unsetteld);
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
            // prepare client data
            GCHandle astHandle = (GCHandle)data;
            NativeClass thisClass = astHandle.Target as NativeClass;

            CXCursorKind kind = clang.getCursorKind(cursor);
            switch(kind)
            {
                case CXCursorKind.CXCursor_CXXMethod:
                    ProcessMethod(thisClass, cursor, parent);
                    break;
                case CXCursorKind.CXCursor_FieldDecl:
                    break;
                case CXCursorKind.CXCursor_CXXBaseSpecifier:
                    ProcessBaseClass(thisClass, cursor, parent);
                    break;
                default:
                    break;
            }

            return CXChildVisitResult.CXChildVisit_Continue;
        }

        private void ProcessBaseClass(NativeClass thisClass, CXCursor cursor, CXCursor parent)
        {
            CXCursor refCursor = clang.getCursorReferenced(cursor);
            string name = clang.getCursorSpelling(refCursor).ToString();
            bool unsetteld = false;
            NativeClass @class = AST_.GetClass(name, StructOrClass.InDoubt, out unsetteld);
            thisClass.AddBaseClass(@class);
        }

        private void ProcessMethod(NativeClass thisClass, CXCursor cursor, CXCursor parent)
        {
            uint i = clang.getNumOverloadedDecls(cursor);
            i = clang.getNumOverloadedDecls(parent);
        }
    }
}
