using System;
using System.IO;
using DotLiquid;

namespace clangen
{
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
            //AST ast = p.ParseWithClangArgs(args);
            return;
        }
    }
}
