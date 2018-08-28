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
    }

    public class TemplateParameter
    {
        public string Name;
        public bool IsTemplate;
        public TemplateProto Template;
    }
}
