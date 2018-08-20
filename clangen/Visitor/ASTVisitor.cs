using System;
using System.Runtime.InteropServices;
using ClangSharp;

namespace clangen
{
    class ASTVisitor
    {
        public AST Visit(CXTranslationUnit TU)
        {
            // root cursor
            CXCursor root = clang.getTranslationUnitCursor(TU);

            // AST Tree
            AST astTree = new AST();
            GCHandle astHandle = GCHandle.Alloc(astTree);

            // deep iterate
            clang.visitChildren(
                root, 
                Visitor, 
                new CXClientData((IntPtr)astHandle));

            return astTree;
        }

        // AST traverse
        private CXChildVisitResult Visitor(CXCursor cursor, CXCursor parent, IntPtr data)
        {
            // skip system headers
            CXSourceLocation location = clang.getCursorLocation(cursor);
            if (clang.Location_isFromMainFile(location) == 0)
                return CXChildVisitResult.CXChildVisit_Continue;

            // prepare client data
            GCHandle astHandle = (GCHandle)data;
            AST ast = astHandle.Target as AST;

            // get visitor
            IASTVisitor visitor = null;
            switch (cursor.kind)
            {
                // dealing with class
                case CXCursorKind.CXCursor_StructDecl:
                case CXCursorKind.CXCursor_ClassDecl:
                    visitor = new ClassVisitor(ast);
                    break;
                // dealing with enum type            
                case CXCursorKind.CXCursor_EnumDecl:
                    break;
                // dealing with function
                case CXCursorKind.CXCursor_FunctionDecl:
                    break;
                // TODO ... any other
                default:
                    break;
            }

            if(visitor != null)
            {
                if(!visitor.DoVisit(cursor, parent))
                {
                    return CXChildVisitResult.CXChildVisit_Break;
                }
            }

            // deep already in sub visitors
            return CXChildVisitResult.CXChildVisit_Continue;
        }
    }
}
