using System;
using System.Collections.Generic;
using System.Text;

namespace clangen
{
    public class ClassTemplate
    {
        public TemplateProto TP { get; }
        public string Name { get; }
        public string Spelling { get; set; }
        public bool Parsed { get; set; }

        public ClassTemplate(string name, TemplateProto proto)
        {
            Name = name;
            TP = proto;
        }
    }
}
