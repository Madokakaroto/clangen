using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace clangen
{
    public class NativeClass
    {
        // auto property
        public string Name { get; }
        public bool Parsed { get; set; } = false;
        public StructOrClass ClassTag { get; set; } = StructOrClass.InDoubt;
        public bool IsFinal { get; set; } = false;
        public bool IsAbstract { get; set; } = false;
        public bool IsVirtualBase { get; set; } = false;

        // for template instantiation
        public bool IsTemplateInstance { get; private set; } = false;
        public ClassTemplate InstanceOf { get; private set; } = null;
        public NativeType[] TemplateParameters { get; private set; } = null;

        // for embedded class
        public bool IsEmbedded { get; set; } = false;
        public NativeClass OwnerClass { get; set; } = null;
        public AccessSpecifier Access { get; set; } = AccessSpecifier.Private;

        // private data
        public List<BaseClass> BaseClasses { get; } = new List<BaseClass>();
        public List<SubClass> SubClasses { get; } = new List<SubClass>();
        public List<MemberType> MemberTypes { get; } = new List<MemberType>();
        public List<Constructor> Constructors { get; } = new List<Constructor>();

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
            BaseClasses.Add(baseClass);
        }

        public void AddSubClass(SubClass subClass)
        {
            SubClasses.Add(subClass);
        }

        public void AddMemberType(MemberType type)
        {
            MemberTypes.Add(type);
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

        public void AddConstructor(Constructor ctor)
        {
            Constructors.Add(ctor);
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
