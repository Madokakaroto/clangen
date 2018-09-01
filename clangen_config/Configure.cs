using System;
using System.IO;
using System.Collections.Generic;
using YamlDotNet;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace clangen
{
    public class Configure
    {
        public static CppConfig GetCppConfig(string configFile)
        {
            string fileContent = File.ReadAllText(configFile);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            CppConfig cppConfig = deserializer.Deserialize<CppConfig>(fileContent);
            cppConfig.Sources = ToAbsolutPath(cppConfig.Sources);
            return cppConfig;
        }

        private static List<string> ToAbsolutPath(List<string> path)
        {
            List<string> filtPath = new List<string>();
            foreach(string p in path)
            {
                if(Path.IsPathRooted(p))
                    filtPath.Add(p);
                else
                    filtPath.Add(Path.GetFullPath(p));
            }
            return filtPath;
        }
    }
}