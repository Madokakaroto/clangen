using System;
using System.Collections.Generic;
using System.Text;
using ClangSharp;

namespace clangen
{
    public class ClassTemplateVisitor : IASTVisitor
    {
        AST AST_;

        public ClassTemplateVisitor(AST ast)
        {
            AST_ = ast;
        }

        public bool DoVisit(CXCursor cursor, CXCursor parent)
        {
            clang.visitChildren(cursor, Visitor, new CXClientData(IntPtr.Zero));
            return true;
        }

        private CXChildVisitResult Visitor(CXCursor cursor, CXCursor parent, IntPtr data)
        {
            CXCursorKind kind = cursor.kind;
            return CXChildVisitResult.CXChildVisit_Continue;
        }
    }
}
