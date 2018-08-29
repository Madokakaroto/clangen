using System.Diagnostics;
using System.Collections.Generic;

namespace clangen
{
    public class TemplateProto
    {
        public List<TemplateParameter> ParametersList { get; private set; } 
            = new List<TemplateParameter>();
        public Dictionary<string, TemplateParameter> ParametersDict { get; private set; }
            = new Dictionary<string, TemplateParameter>();

        public void AddTemplateParameter(TemplateParameter param)
        {
            ParametersList.Add(param);
            if (!param.IsTemplate)
                return;

            Debug.Assert(param.Template != null);
            foreach(KeyValuePair<string, TemplateParameter> entry in param.Template.ParametersDict)
            {
                Debug.Assert(!ParametersDict.ContainsKey(entry.Key));
                ParametersDict.Add(entry.Key, entry.Value);
            }
        }

        public string Signature { get { return GetSignature(false); } }
        public string NamedSignature { get { return GetSignature(true); } }

        public string GetSignature(bool withParamName, bool isTemplateParam = false)
        {
            List<string> tmp = new List<string>();
            foreach (TemplateParameter param in ParametersList)
            {
                tmp.Add(param.GetSignature(withParamName));
            }
            string tmpString = string.Join(", ", tmp);
            if(!isTemplateParam)
                return string.Format("template <{0}>", tmpString);

            return string.Format("template <{0}> class ");
        }
    }

    public class TemplateParameter
    {
        public string Name { get; }
        public NativeType Type{ get; }
        public bool IsTemplate { get; private set;  } = false;
        public TemplateProto Template { get; private set; } = null;

        public string Signature { get { return GetSignature(false); } }
        public string NamedSignature { get { return GetSignature(true); } }
        public bool IsNonType { get { return Type != null; } }

        public TemplateParameter(string name, NativeType type = null)
        {
            Name = name;
            Type = type;
        }

        public string GetSignature(bool withParamName)
        {
            string result;

            // format template parameter signature
            if(IsNonType)
                result = string.Format("{0} ", Type.CollaspedName);
            else if (!IsTemplate)
                result = "typename ";
            else
                result = Template.GetSignature(withParamName, true);

            // add template parameter spelling
            if (withParamName && Name.Length > 0) 
                result += Name;

            return result;
        }
    }
}
