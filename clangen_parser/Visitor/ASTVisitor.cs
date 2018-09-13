using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ClangSharp;

namespace clangen
{
    class ASTVisitor
    {
        static public CXTranslationUnit CurrentTU { get; private set; }
        private List<string> namespaces_ = new List<string>();
        public string CurrentNamespace { get { return string.Join("::", namespaces_); } }

        public AST Visit(CXTranslationUnit TU)
        {
            // set current
            CurrentTU = TU;

            // root cursor
            CXCursor root = clang.getTranslationUnitCursor(TU);

            // AST Tree
            AST astTree = new AST();
            GCHandle astHandle = GCHandle.Alloc(astTree);

            // deep iterate
            clang.visitChildren(
                root, 
                Visitor, 
                new CXClientData((IntPtr)astHandle));

            return astTree;
        }

        // AST traverse
        private CXChildVisitResult Visitor(CXCursor cursor, CXCursor parent, IntPtr data)
        {
            // skip system headers
            CXSourceLocation location = clang.getCursorLocation(cursor);
            if (clang.Location_isFromMainFile(location) == 0)
                return CXChildVisitResult.CXChildVisit_Continue;

            // prepare client data
            GCHandle astHandle = (GCHandle)data;
            AST ast = astHandle.Target as AST;

            // get visitor
            IASTVisitor visitor = null;
            switch (cursor.kind)
            {
                // dealing with class
                case CXCursorKind.CXCursor_StructDecl:
                case CXCursorKind.CXCursor_ClassDecl:
                    visitor = new ClassVisitor(ast);
                    break;
                // dealing with enum type            
                case CXCursorKind.CXCursor_EnumDecl:
                    visitor = new EnumVisitor(ast);
                    break;
                // dealing with function
                case CXCursorKind.CXCursor_FunctionDecl:
                    visitor = new FunctionVisitor(ast);
                    break;
                case CXCursorKind.CXCursor_Namespace:
                    // ignore anonymous namespace
                    if (clang.Cursor_isAnonymous(cursor) != 0)
                        return CXChildVisitResult.CXChildVisit_Continue;

                    ProcessNamespace(cursor, data);
                    return CXChildVisitResult.CXChildVisit_Continue;
                case CXCursorKind.CXCursor_TypeAliasDecl:
                case CXCursorKind.CXCursor_TypedefDecl:
                    //TypeVisitorHelper.GetNativeType(ast, clang.getCursorType(cursor));
                    visitor = new TypeVisitor(ast);
                    break;
                case CXCursorKind.CXCursor_ClassTemplate:
                    visitor = new ClassTemplateVisitor(ast, this, false);
                    break;
                case CXCursorKind.CXCursor_ClassTemplatePartialSpecialization:
                    visitor = new ClassTemplateVisitor(ast, this, true);
                    break;
                case CXCursorKind.CXCursor_FunctionTemplate:
                    break;
                default:
                    // TODO ... any other
                    break;
            }

            if(visitor != null)
            {
                if(!visitor.DoVisit(cursor, parent))
                {
                    return CXChildVisitResult.CXChildVisit_Break;
                }
            }

            // deep iteratoring in sub visitor
            return CXChildVisitResult.CXChildVisit_Continue;
        }

        private void ProcessNamespace(CXCursor cursor, IntPtr data)
        {
            string namespaceName = clang.getCursorSpelling(cursor).ToString();
            // push namespace
            namespaces_.Add(namespaceName);
            // visit child
            clang.visitChildren(cursor, Visitor, new CXClientData(data));
            // pop namespace
            int size = namespaces_.Count - 1;
            namespaces_.RemoveAt(size);
        }

        public string GetCurrentScopeName(string spelling)
        {
            List<string> NameGenList = new List<string>();
            foreach(string ns in namespaces_)
            {
                NameGenList.Add(ns);
            }
            NameGenList.Add(spelling);
            return string.Join("::", NameGenList);
        }

        public static List<string> GetCursorTokens(CXCursor cursor)
        {
            // get liter-string from token
            CXSourceRange range = clang.getCursorExtent(cursor);
            IntPtr tokens = IntPtr.Zero;
            uint numToken;
            List<string> result = new List<string>();
            clang.tokenize(CurrentTU, range, out tokens, out numToken);
            IntPtr tmp = tokens;
            for (uint loop = 0; loop < numToken; ++loop, tmp = IntPtr.Add(tmp, Marshal.SizeOf<CXToken>()))
            {
                CXToken token = Marshal.PtrToStructure<CXToken>(tmp);
                result.Add(clang.getTokenSpelling(CurrentTU, token).ToString());
            }
            clang.disposeTokens(CurrentTU, tokens, numToken);
            return result;
        }
    }
}
