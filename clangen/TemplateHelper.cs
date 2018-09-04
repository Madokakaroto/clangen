using System;
using System.IO;
using DotLiquid;

namespace clangen
{
    class TemplateHelper
    {
        public static Template ParseTemplate(string path)
        {
            string source = File.ReadAllText(path);
            Template enumTemplate = Template.Parse(source);
            return enumTemplate;
        }
    }
}
