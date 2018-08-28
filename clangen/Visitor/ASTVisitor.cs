using System;
using System.Runtime.InteropServices;
using ClangSharp;

namespace clangen
{
    class ASTVisitor
    {
        static public CXTranslationUnit CurrentTU { get; private set; }
        private string Namespace = "";

        public AST Visit(CXTranslationUnit TU)
        {
            // set current
            CurrentTU = TU;

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
                    visitor = new EnumVisitor(ast);
                    break;
                // dealing with function
                case CXCursorKind.CXCursor_FunctionDecl:
                    visitor = new FunctionVisitor(ast, Namespace);
                    break;
                case CXCursorKind.CXCursor_Namespace:
                    // ignore anonymous namespace
                    if (clang.Cursor_isAnonymous(cursor) != 0)
                        return CXChildVisitResult.CXChildVisit_Continue;
                    else
                    {
                        string NS = clang.getCursorSpelling(parent).ToString();

                        if(Namespace.Length == 0)
                            Namespace += NS;
                        else
                            Namespace = Namespace + "::" + NS;
                    }
                    return CXChildVisitResult.CXChildVisit_Recurse;
                case CXCursorKind.CXCursor_TypeAliasDecl:
                case CXCursorKind.CXCursor_TypedefDecl:
                    TypeVisitHelper.GetNativeType(ast, clang.getCursorType(cursor));
                    break;
                case CXCursorKind.CXCursor_ClassTemplate:
                    visitor = new ClassTemplateVisitor(ast);
                    break;
                case CXCursorKind.CXCursor_ClassTemplatePartialSpecialization:
                    break;
                case CXCursorKind.CXCursor_FunctionTemplate:
                    break;
                default:
                    // TODO ... any other
                    break;
            }

            if(visitor != null)
            {
                if(!visitor.DoVisit(cursor, parent))
                {
                    return CXChildVisitResult.CXChildVisit_Break;
                }
            }

            // deep iteratoring in sub visitor
            return CXChildVisitResult.CXChildVisit_Continue;
        }
    }
}
