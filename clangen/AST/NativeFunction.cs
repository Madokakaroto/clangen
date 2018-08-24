using System.Collections.Generic;
using System.Linq;

namespace clangen
{
    public class FunctionParameter
    {
        public string Name { get; set; }
        public NativeType Type { get; set; }
        public string DefaultValue { get; set; }
    }

    public class NativeFunction
    {
        public string Name { get; }
        public bool IsOverload { get; }

        public List<string> Namespaces { get; } = new List<string>();
        public NativeType ReturnType { get; set; } = null;
        public List<FunctionParameter> ParamList { get; } = new List<FunctionParameter>();

        public NativeFunction(string name, bool isOverload = false)
        {
            Name = name;
            IsOverload = isOverload;
        }

        public bool IsVoidReturn()
        {
            return ReturnType == null;
        }

        public bool HasParam()
        {
            return ParamList.Count() != 0;
        }

        public void AddParam(FunctionParameter param)
        {
            ParamList.Add(param);
        }

        public void PushNamespace(string @namespace)
        {
            Namespaces.Add(@namespace);
        }

        bool IsGlobal()
        {
            return Namespaces.Count() == 0;
        }
    }
}
