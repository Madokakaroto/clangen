using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace clangen
{
    public enum StructOrClass
    {
        Struct = -1,
        InDoubt = 0,
        Class = 1,
    }

    public enum AccessSpecifier
    {
        Invalid,
        Public,
        Protected,
        Private
    }

    public struct BaseClass
    {
        public NativeClass Class;
        public bool IsVirtual;
        public AccessSpecifier Access;
    }

    public class NativeClass
    {
        // auto property
        public string Name { get; }
        public StructOrClass ClassTag { get; set; } = StructOrClass.InDoubt;
        public bool IsFinal { get; set; } = false;
        public bool IsAbstract { get; set; } = false;
        public bool IsVirtualBae { get; set; } = false;
        public bool IsTemplateInstance { get; private set; } = false;
        public NativeType[] TemplateParameters { get; private set; } = null;
        // private data
        private List<BaseClass> baseClasses_ = new List<BaseClass>();
        private Dictionary<string, List<MemberFunction>> memberFunctions_
            = new Dictionary<string, List<MemberFunction>>();

        public NativeClass(string name)
        {
            Name = name;
        }

        public void AddBaseClass(BaseClass baseClass)
        {
            baseClasses_.Add(baseClass);
        }

        public void AddMethod(MemberFunction func)
        {
            string funcName = func.Name;
            if (memberFunctions_.ContainsKey(funcName))
            {
                if (memberFunctions_[func.Name].Count() == 1)
                    memberFunctions_[func.Name].ElementAt(0).IsOverload = true;

                func.IsOverload = true;
            }
            else
            {
                memberFunctions_.Add(funcName, new List<MemberFunction>());
            }
            memberFunctions_[funcName].Add(func);
        }

        public void SetTemplateParameterCount(uint count)
        {
            IsTemplateInstance = true;
            TemplateParameters = new NativeType[count];
        }

        public void SetTemplateParameter(uint index, NativeType type)
        {
            TemplateParameters[index] = type;
        }
    }
}
