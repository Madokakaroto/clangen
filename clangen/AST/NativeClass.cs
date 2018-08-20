using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace clangen
{
    enum StructOrClass
    {
        Struct = -1,
        InDoubt = 0,
        Class = 1,
    }

    class NativeClass
    {
        // auto property
        public string Name { get; }
        public StructOrClass ClassTag { get; set; }

        // private data
        private List<NativeClass> baseClasses_;
        private Dictionary<string, List<MemberFunction>> memberFunctions_; 

        public NativeClass(string name, StructOrClass classTag = StructOrClass.InDoubt)
        {
            Name = name;
            ClassTag = classTag;

            baseClasses_ = new List<NativeClass>();
            memberFunctions_ = new Dictionary<string, List<MemberFunction>>();
        }

        public void AddBaseClass(NativeClass @class)
        {
            baseClasses_.Add(@class);
        }

        public void AddMethod(MemberFunction func)
        {
            if(memberFunctions_.ContainsKey(func.Name))
            {
                memberFunctions_[func.Name].ElementAt(0).
            }
        }
    }
}
