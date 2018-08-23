using System;
using System.Diagnostics;
namespace clangen
{
    public class Method
    {
        private NativeClass class_;
        public string Name { get; }
        public bool IsStatic { get; }
        public bool IsConst { get; }
        public bool IsVirtual { get; }
        public bool IsAbstract { get; }

        public bool IsOverload { get; set; }
        public bool IsOverride { get; set; }

        public Method(
            NativeClass @class,
            string name, 
            bool isStatic,
            bool isConst,
            bool isVirtual,
            bool isAbstract)
        {
            Debug.Assert(!(isStatic && (isConst || isVirtual || isAbstract)));
            class_ = @class;
            Name = name;
            IsStatic = isStatic;
            IsConst = isConst;
            IsVirtual = isVirtual;
            IsAbstract = isAbstract;
        }
        
    }
}
