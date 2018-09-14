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
                Console.WriteLine("Failed to parse Translation Unit!");
                return null;
            }

            bool fatal = false;
            var numDiagnostics = clang.getNumDiagnostics(TU);
            for(uint loop = 0; loop < numDiagnostics; ++loop)
            {
                fatal |= DealingWithDiagnostic(clang.getDiagnostic(TU, loop));
            }
            if (fatal)
                return null;

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

        private bool DealingWithDiagnostic(CXDiagnostic d)
        {
            // error spelling
            string spelling = clang.getDiagnosticSpelling(d).ToString();

            // category text
            string categoryText = clang.getDiagnosticCategoryText(d).ToString();

            // severity text
            CXDiagnosticSeverity severity = clang.getDiagnosticSeverity(d);
            string severityStr = ClangTraits.ToString(severity);

            // source location
            CXSourceLocation location = clang.getDiagnosticLocation(d);
            CXFile file = new CXFile(IntPtr.Zero);
            clang.getInstantiationLocation(
                location,
                out file,
                out uint line,
                out uint column,
                out uint offset);

            string fileName = clang.getFileName(file).ToString();
            clang.disposeDiagnostic(d);

            string errorString = string.Format("{0}: {1}-{2}, IN {3}, line: {4}, column: {5}",
                severityStr, spelling, categoryText, fileName, line, column);
            return ClangTraits.IsFatal(severity);
        }
    }
}
