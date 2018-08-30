using System;
using System.Collections.Generic;
using System.Text;

namespace clangen
{
    public class ClassTemplate
    {
        public TemplateProto TP { get; }
        public string ID { get; }
        public string Name { get; set; }
        public string Spelling { get; set; }
        public bool Parsed { get; set; }

        public ClassTemplate(string id, TemplateProto proto)
        {
            ID = id;
            TP = proto;
        }
    }
}
