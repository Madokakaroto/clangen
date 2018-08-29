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
        private BasicType type_;
        public bool IsTemplate { get; private set;  } = false;
        public TemplateProto Template { get; private set; } = null;

        public string Signature { get { return GetSignature(false); } }
        public string NamedSignature { get { return GetSignature(true); } }
        public bool IsNonType { get { return type_ != BasicType.Unknown; } }

        TemplateParameter(string name, BasicType type)
        {
            //Debug.Assert()

            Name = name;

        }

        public string GetSignature(bool withParamName)
        {
            string result;
            if (!IsTemplate)
                result = "typename ";
            else
                result = Template.GetSignature(withParamName, true);
            if (withParamName) result += Name;
            return result;
        }
    }
}
