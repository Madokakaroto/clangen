using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clangen
{
    class NativeClassDrop : DotLiquid.Drop
    {
        public string Name { get; }
        public string UnscopedName { get; }
        public object Extra { get; }

        public NativeClassDrop(NativeClass nativeClass, object extra)
        {

        }
    }
}
