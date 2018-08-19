using System;
using clangen;

namespace ClangSharp
{
    interface IASTVisitor
    {
        bool DoVisit(CXCursor cursor, CXCursor parent);
    }
}
