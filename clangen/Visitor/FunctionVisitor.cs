using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClangSharp;

namespace clangen
{
    class FunctionVisitor : IASTVisitor
    {
        private AST AST_;

        public FunctionVisitor(AST ast)
        {
            AST_ = ast;
        }

        public bool DoVisit(CXCursor cursor, CXCursor parent)
        {
            return true;
        }

        private CXChildVisitResult Visitor(CXCursor cursor, CXCursor parent, IntPtr data)
        {
            string funcName = clang.getCursorSpelling(cursor).ToString();
            return CXChildVisitResult.CXChildVisit_Continue;
        }
    }
}
