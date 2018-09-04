using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clangen.cli
{
    class Property
    {
        public string Propname { get; set; }
        public string Set { get; set; }
        public string Get { get; set; }
        public bool Browsable { get; set; } = false;
        public string Category { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }
    }

    class ClassAttribute
    {
        public string Category { get; set; }
        public string DisplayName { get; set; }
        public bool Browsable { get; set; } = false;
    }

    class ClassExtra
    {
        public Dictionary<string, Property> Properties;
        public ClassAttribute Attribute;
        public List<string> Skips;
        public Dictionary<string, string> Renames;
        public bool SkipBaseClass { get; set; } = false;
        public bool SkipAsBaseClass { get; set; } = false;
    }

    class Config
    {
        public Dictionary<string, ClassExtra> Classes;
    }

}
