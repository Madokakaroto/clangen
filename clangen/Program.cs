using System;
using System.IO;
using DotLiquid;

namespace clangen
{
    public class User
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class UserDrop : Drop
    {
        private readonly User _user;

        public string Name
        {
            get { return _user.Name; }
        }

        public UserDrop(User user)
        {
            _user = user;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length < 1)
            {
                Console.WriteLine("usage: clangen.exe <config_file>");
                return;
            }
            
            if(args.Length > 1)
            {
                Console.WriteLine("Warnning: Arguments exceed the first one will be ignored!");
            }
            
            string filePath = args[0];
            if (!File.Exists(filePath))
            {
                string error = string.Format("Configure file: {0} not found!", filePath);
                Console.WriteLine(error);
                return;
            }

            string libclang_path = Environment.CurrentDirectory;
            int index = libclang_path.LastIndexOf('\\');
            libclang_path = libclang_path.Substring(0, index);
            Parser p = new Parser(libclang_path);
            CppConfig cppConfig = Configure.GetCppConfig(filePath);
            AST ast = p.ParseWithClangArgs(cppConfig);

            Enumeration @enum = ast.GetEnum("foo::fee");

            // create template
            Template tmpl = ProcessTemplate();
            string result = tmpl.Render(Hash.FromAnonymousObject(new
            {
                @enum
            }));
        }

        static Template ProcessTemplate()
        {
            string source = File.ReadAllText("../../Test/TestCase/Templates/enum_item.cpp");
            Template enumTemplate = Template.Parse(source);
            return enumTemplate;
        }
    }
}
