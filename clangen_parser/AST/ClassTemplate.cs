using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace clangen
{
    public enum TemplateParameterKind
    {
        Type,
        NoneType,
        Template,
        Dependent
    }

    public class TemplateProto
    {
        public List<TemplateParameter> ParametersList { get; }
            = new List<TemplateParameter>();

        public void AddTemplateParameter(TemplateParameter param)
        {
            ParametersList.Add(param);
        }

        public TemplateParameter GetTemplateParameter(uint index)
        {
            return ParametersList[(int)index];
        }

        public TemplateParameter GetTemplateParameter(string name)
        {
            foreach(TemplateParameter param in ParametersList)
            {
                if (param.Name == name)
                    return param;
            }
            return null;
        }

        public int GetTemplateParameterIndex(string name)
        {
            for(int loop = 0; loop < ParametersList.Count; ++loop)
            {
                if (ParametersList[loop].Name == name)
                    return loop;
            }
            return -1;
        }

        public int Count { get { return ParametersList.Count; } }
        public string Signature { get { return GetSignature(false); } }
        public string NamedSignature { get { return GetSignature(true); } }

        public string GetSignature(bool withParamName, bool isTemplateParam = false)
        {
            List<string> tmp = new List<string>();
            foreach(TemplateParameter param in ParametersList)
            {
                tmp.Add(param.GetSignature(withParamName));
            }
            string str = string.Join(", ", tmp);
            if (!isTemplateParam)
                return string.Format("template <{0}>", str);

            return string.Format("template <{0}> class", str);
        }
    }

    public class TemplateNonTypeParam
    {
        public object NoneType { get; set; }
        public string DefaultLiteral { get; set; }
    }

    public class TemplateParameter
    {
        public string Name { get; }
        public TemplateParameterKind Kind { get; }
        public bool IsVariadic { get; }
        public object Extra { get; private set; }

        public TemplateParameter(string name, TemplateParameterKind kind, bool isVariadic)
        {
            Name = name;
            Kind = kind;
            IsVariadic = isVariadic;
        }

        public void SetExtra(NativeType extra, string literal)
        {
            Debug.Assert(TemplateParameterKind.NoneType == Kind);
            Debug.Assert(ASTTraits.IsIntegral(extra) || ASTTraits.IsEnum(extra));
            Extra = new TemplateNonTypeParam
            {
                NoneType = extra,
                DefaultLiteral = literal
            };
        }

        public void SetExtra(TemplateParameter extra, string literal)
        {
            Debug.Assert(TemplateParameterKind.Dependent == Kind);
            Debug.Assert(TemplateParameterKind.Type == extra.Kind);
            Extra = new TemplateNonTypeParam
            {
                NoneType = extra,
                DefaultLiteral = literal
            };
        }

        public void SetExtra(TemplateProto proto)
        {
            Debug.Assert(Kind == TemplateParameterKind.Template);
            Extra = proto;
        }

        public string GetSignature(bool withParamName)
        {
            string result;

            // format template parameter ignature
            if (TemplateParameterKind.Type == Kind)
                result = "typename";
            else if (TemplateParameterKind.NoneType == Kind)
                result = string.Format("{0} ", (Extra as NativeType).CollapsedName);
            else if (TemplateParameterKind.Dependent == Kind)
                result = (Extra as TemplateParameter).Name;
            else
                result = (Extra as TemplateProto).GetSignature(withParamName, true);

            // add template parameter spelling
            if (withParamName && Name.Length > 0)
                result += " " + Name;

            return result;
        }
    }

    public class ClassTemplate
    {
        public TemplateProto TP { get; set; }
        public string ID { get; }
        public string Name { get; set; }
        public string Spelling { get; set; }
        public bool Parsed { get; set; }

        public ClassTemplate(string id)
        {
            ID = id;
        }
    }
}
