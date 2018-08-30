using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ClangSharp;

namespace clangen
{
    class FunctionVisitor : IASTVisitor
    {
        private AST AST_;
        //private string namespace_;

        public FunctionVisitor(AST ast)
        {
            AST_ = ast;
        }

        public bool DoVisit(CXCursor cursor, CXCursor parent)
        {
            string cursorName = clang.getCursorSpelling(cursor).ToString();
            string functionName = cursorName;
            string functionTypeName = clang.getCursorDisplayName(cursor).ToString();

            NativeFunction function = AST_.GetFunction(functionName, functionTypeName);
            if(!function.Parsed)
            {
                // proces result type
                CXType resultType = clang.getCursorResultType(cursor);
                function.ResultType = TypeVisitor.GetNativeType(AST_, resultType);

                // create IntPtr for context
                GCHandle funcHandle = GCHandle.Alloc(function);

                // visit children
                clang.visitChildren(cursor, Visitor, new CXClientData((IntPtr)funcHandle));
            }

            return true;
        }

        private CXChildVisitResult Visitor(CXCursor cursor, CXCursor parent, IntPtr data)
        {
            if(CXCursorKind.CXCursor_ParmDecl == cursor.kind)
            {
                // prepare client data
                GCHandle funcHandle = (GCHandle)data;
                NativeFunction thisFunc = funcHandle.Target as NativeFunction;

                CXType type = clang.getCursorType(cursor);

                FunctionParameter param = new FunctionParameter
                {
                    Name = clang.getCursorSpelling(cursor).ToString(),
                    Type = TypeVisitor.GetNativeType(AST_, type)
                };

                clang.visitChildren(cursor, (CXCursor c, CXCursor p, IntPtr d) => 
                {
                    if(ClangTraits.IsLiteralCursor(c))
                    {
                        // get liter-string from token
                        List<string> tokens = ASTVisitor.GetCursorTokens(c);

                        // set default literal
                        param.DefaultValue = string.Concat(tokens);
                    }
                    return CXChildVisitResult.CXChildVisit_Continue;
                }, new CXClientData(IntPtr.Zero));

                thisFunc.AddParameter(param);
            }

            return CXChildVisitResult.CXChildVisit_Recurse;
        }
    }
}
