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
        public bool IsTemplateInstance { get; set; }

        // private data
        private List<NativeClass> baseClasses_;
        private Dictionary<string, List<MemberFunction>> memberFunctions_;

        public NativeClass(string name)
        {
            Name = name;
            ClassTag = StructOrClass.InDoubt;
            IsTemplateInstance = false;
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
                if(memberFunctions_[func.Name].Count() == 1)
                    memberFunctions_[func.Name].ElementAt(0).IsOverload = true;

                func.IsOverload = true;
            }
            memberFunctions_[func.Name].Add(func);
        }
    }
}
