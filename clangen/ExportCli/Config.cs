using System.IO;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace clangen.cli
{
    class PropertyT
    {
        public string Property { get; set; }
        public string Set { get; set; }
        public string Get { get; set; }
        public bool Browsable { get; set; }
        public bool ReadOnly { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public string Editor { get; set; }
        public string Range { get; set; }
    }

    class ClassAttribute
    {
        public string Category { get; set; }
        public string DisplayName { get; set; }
        public bool Browsable { get; set; } = false;
    }

    class Rename
    {
        public string From { get; set; }
        public string To { get; set; }
    }

    class ClassExtra
    {
        public List<PropertyT> Properties { get; set; } = new List<PropertyT>();
        public ClassAttribute Attributes { get; set; } = new ClassAttribute();
        public List<string> Skips { get; set; } = new List<string>();
        public List<Rename> Renames = new List<Rename>();
        public bool SkipBaseClass { get; set; } = false;
    }

    class Config
    {
        public string DefaultNamespace { get; set; }
        public string ExportNamespace { get; set; }
        public Dictionary<string, ClassExtra> Classes { get; set; }
        public List<string> Enums { get; set; }
        public List<string> Typedefs { get; set; }
        public HashSet<string> SkipClasses { get; set; }

        Config() { }

        public static Config GetConfig(string path)
        {
            string fileContent = File.ReadAllText(path);
            var deserializer = new DeserializerBuilder().Build();
            return deserializer.Deserialize<Config>(fileContent);
        }
    }

}
