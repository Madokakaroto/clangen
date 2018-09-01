using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using ClangSharp;

namespace clangen
{
    //static class Extensions
    //{
    //    /// <summary>
    //    /// Get the array slice between the two indexes.
    //    /// ... Inclusive for start index, exclusive for end index.
    //    /// </summary>
    //    public static List<T> Slice<T>(this List<T> source, int start, int end)
    //    {
    //        // Handles negative ends.
    //        if (end < 0)
    //        {
    //            end = source.Count + end;
    //        }
    //        int len = end - start;
    //
    //        // Return new array.
    //        List<T> res = new List<T>();
    //        for (int i = 0; i < len; i++)
    //        {
    //            res[i] = source[i + start];
    //        }
    //        return res;
    //    }
    //
    //    public static List<T> Slice<T>(this List<T> source, int start)
    //    {
    //        return Slice(source, start, source.Count);
    //    }
    //}

    public class Parser
    {
        public Parser(string libPath)
        {
            SetDllDirectory(libPath);
        }

        public AST ParseWithClangArgs(CppConfig config)
        {
            if(config.Sources.Count == 0)
            {
                 Console.WriteLine("No input sources or includes");
                return null;
            }

            // the index object
            CXIndex Index = clang.createIndex(0, 0);

            // prepare some vars for parse
            uint option = clang.defaultEditingTranslationUnitOptions()
                | (uint)CXTranslationUnit_Flags.CXTranslationUnit_SkipFunctionBodies;
            CXUnsavedFile unsavedFile = new CXUnsavedFile();
            
            CXTranslationUnit TU;
            var error = clang.parseTranslationUnit2(Index, config.Sources[0], config.Extras, config.Extras.Length, out unsavedFile, 0,
                option, out TU);
            if (error != CXErrorCode.CXError_Success)
            {
                Console.WriteLine("Error: " + error);
                var numDiagnostics = clang.getNumDiagnostics(TU);

                for (uint i = 0; i < numDiagnostics; ++i)
                {
                    var diagnostic = clang.getDiagnostic(TU, i);
                    Console.WriteLine(clang.getDiagnosticSpelling(diagnostic).ToString());
                    clang.disposeDiagnostic(diagnostic);
                }
                return null;
            }

            ASTVisitor visitor = new ASTVisitor();
            AST ast = visitor.Visit(TU);
            clang.disposeIndex(Index);
            return ast;
        }

        private static long GetFileSize(string FileName)
        {
            FileInfo fileInfo = new FileInfo(FileName);
            return fileInfo.Length;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetDllDirectory(string lpPathName);
    }
}
