using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClangSharp;

namespace clangen
{
    class EnumVisitor : IASTVisitor
    {
        private ASTVisitor ASTVisitor_;

        public EnumVisitor(ASTVisitor visitor)
        {
            ASTVisitor_ = visitor;
        }

        public bool DoVisit(CXCursor cursor, CXCursor parent)
        {
            return true;
        }
    }
}
