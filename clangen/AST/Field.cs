using System;
using System.Collections.Generic;
using System.Text;

namespace clangen
{
    public struct Field
    {
        public AccessSpecifier Access;
        public bool IsStatic;
        public NativeType Type;
    }
}
