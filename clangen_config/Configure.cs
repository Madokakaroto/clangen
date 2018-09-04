using System;
using System.IO;
using System.Collections.Generic;
using YamlDotNet;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace clangen
{
    public class UserTest111
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
    }


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


            //test

            Dictionary<string, UserTest111> dict = new Dictionary<string, UserTest111>();
            dict.Add("1", new UserTest111 { Name = "123", Email = "123@qq.com" });
            dict.Add("2", new UserTest111 { Name = "234", Email = "123@qq.com" });
            dict.Add("3", new UserTest111 { Name = "345", Email = "123@qq.com" });

            var deserializer1 = new DeserializerBuilder().Build();
            var serializer = new SerializerBuilder().Build();
            string yaml = serializer.Serialize(dict);
            yaml = @"1:
  Name: 123
  Email: 123@qq.com
2: 
3:
  Name: 345
  Email: 123@qq.com";

            Dictionary<string, UserTest111> ddd = deserializer1.Deserialize<Dictionary<string, UserTest111>>(yaml);


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