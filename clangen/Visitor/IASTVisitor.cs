using System;
using clangen;
using ClangSharp;

namespace clangen
{
    interface IASTVisitor
    {
        bool DoVisit(CXCursor cursor, CXCursor parent);
    }
}
