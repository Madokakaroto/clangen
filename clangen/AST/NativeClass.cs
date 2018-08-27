using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

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
        public bool IsVirtualBase { get; set; } = false;

        // for full specialization
        public bool IsTemplateInstance { get; private set; } = false;
        public NativeType[] TemplateParameters { get; private set; } = null;

        // for sub class
        public bool IsSubClass { get; set; } = false;
        public AccessSpecifier SubAccess { get; set; } = AccessSpecifier.Private;

        // private data
        private List<BaseClass> baseClasses_ = new List<BaseClass>();
        private Dictionary<string, List<Method>> methods_
            = new Dictionary<string, List<Method>>();
        private Dictionary<string, Field> fields_
            = new Dictionary<string, Field>();

        public NativeClass(string name)
        {
            Name = name;
        }

        public void AddBaseClass(BaseClass baseClass)
        {
            baseClasses_.Add(baseClass);
        }

        public void AddMethod(Method func)
        {
            string funcName = func.Name;
            if (methods_.ContainsKey(funcName))
            {
                if (methods_[func.Name].Count() == 1)
                    methods_[func.Name].ElementAt(0).IsOverload = true;

                func.IsOverload = true;
            }
            else
            {
                methods_.Add(funcName, new List<Method>());
            }
            methods_[funcName].Add(func);
        }

        public void AddField(string name, Field field)
        {
            Debug.Assert(!fields_.ContainsKey(name));
            if(!fields_.ContainsKey(name))
            {
                fields_.Add(name, field);
            }
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
