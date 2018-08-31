using System.Collections.Generic;
using System.Linq;

namespace clangen
{
    public class NativeFunction
    {
        public string Name { get; }
        public bool Parsed { get; set; } = false;
        public string TypeString { get; }
        public bool IsOverload { get; set; }

        public NativeType ResultType { get; set; } = null;
        public List<FunctionParameter> ParamList { get; } = new List<FunctionParameter>();

        public NativeFunction(string name, string type, bool isOverload = false)
        {
            Name = name;
            TypeString = type;
            IsOverload = isOverload;
        }

        public bool HasParam()
        {
            return ParamList.Count() != 0;
        }

        public void AddParameter(FunctionParameter param)
        {
            ParamList.Add(param);
        }
    }
}
