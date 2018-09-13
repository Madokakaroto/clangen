using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
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

            //NativeType type = ast.GetType(
            //    "std::map<int, double, std::less<int>, std::allocator<std::pair<const int, double>>");

            //ASTTraits.IsInstanceOf(type, "std::map");
            //NativeType keyType = ASTTraits.KeyType(type);
            //NativeType mappedType = ASTTraits.MappedType(type);
            //Debug.Assert(ASTTraits.IsSameType(keyType, mappedType));
        }
    }
}
