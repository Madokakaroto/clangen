﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using ClangSharp;

namespace clangen
{
    static class Extensions
    {
        /// <summary>
        /// Get the array slice between the two indexes.
        /// ... Inclusive for start index, exclusive for end index.
        /// </summary>
        public static T[] Slice<T>(this T[] source, int start, int end)
        {
            // Handles negative ends.
            if (end < 0)
            {
                end = source.Length + end;
            }
            int len = end - start;

            // Return new array.
            T[] res = new T[len];
            for (int i = 0; i < len; i++)
            {
                res[i] = source[i + start];
            }
            return res;
        }

        public static T[] Slice<T>(this T[] source, int start)
        {
            return Slice(source, start, source.Length);
        }
    }

    public class Parser
    {
        public Parser(string libPath)
        {
            SetDllDirectory(libPath);
        }

        public AST ParseWithClangArgs(string[] args)
        {
            if (args.Length < 1 || !File.Exists(args[0]))
            {
                Console.WriteLine("Invalid Arguments");
                return null;
            }

            CXIndex Index = clang.createIndex(0, 0);

            CXTranslationUnit TU;
            string[] @params = args.Slice(1);

            uint option = clang.defaultEditingTranslationUnitOptions()
                | (uint)CXTranslationUnit_Flags.CXTranslationUnit_SkipFunctionBodies;

            CXUnsavedFile unsavedFile = new CXUnsavedFile();
            var error = clang.parseTranslationUnit2(Index, args[0], @params, @params.Length, out unsavedFile, 0,
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
            return visitor.Visit(TU); ;
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
