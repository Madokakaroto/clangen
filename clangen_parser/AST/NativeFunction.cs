using System.Collections.Generic;
using System.Linq;

namespace clangen
{
    public class FunctionProto
    {
        public NativeType ResultType { get; set; }
        public List<FunctionParameter> ParamList { get; } = new List<FunctionParameter>();

        public bool HasParam()
        {
            return ParamList.Count != 0;
        }

        public void AddParameter(FunctionParameter param)
        {
            ParamList.Add(param);
        }
    }


    public class NativeFunction
    {
        public string Name { get; }
        public string UnscopedName { get; set; }
        public bool Parsed { get; set; } = false;
        public string TypeString { get; }
        public bool IsOverload { get; set; }
        public FunctionProto Proto { get; set; } = new FunctionProto();

        public NativeFunction(string name, string type, bool isOverload = false)
        {
            Name = name;
            TypeString = type;
            IsOverload = isOverload;
        }
    }
}
