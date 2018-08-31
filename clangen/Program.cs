using System;
using DotLiquid;

namespace clangen
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser p = new Parser();
            AST ast = p.ParseWithClangArgs(args);

            return;
        }
    }
}
