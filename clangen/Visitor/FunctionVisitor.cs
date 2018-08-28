using System;
using System.Runtime.InteropServices;
using ClangSharp;

namespace clangen
{
    class FunctionVisitor : IASTVisitor
    {
        private AST AST_;
        private string namespace_;

        public FunctionVisitor(AST ast, string NS)
        {
            AST_ = ast;
            namespace_ = NS;
        }

        public bool DoVisit(CXCursor cursor, CXCursor parent)
        {
            string cursorName = clang.getCursorSpelling(cursor).ToString();
            string functionName = namespace_ + cursorName;
            string functionTypeName = clang.getCursorDisplayName(cursor).ToString();

            NativeFunction function = AST_.GetFunction(functionName, functionTypeName);
            if(!function.Parsed)
            {
                // proces result type
                CXType resultType = clang.getCursorResultType(cursor);
                function.ResultType = TypeVisitHelper.GetNativeType(AST_, resultType);

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
                    Type = TypeVisitHelper.GetNativeType(AST_, type)
                };

                clang.visitChildren(cursor, (CXCursor c, CXCursor p, IntPtr d) => 
                {
                    if(ClangTraits.IsLiteralCursor(c))
                    {
                        // get liter-string from token
                        CXSourceRange range = clang.getCursorExtent(c);
                        IntPtr tokens;
                        uint numToken;
                        string liter = "";
                        clang.tokenize(ASTVisitor.CurrentTU, range, out tokens, out numToken);
                        IntPtr tmp = tokens;
                        for (uint loop = 0; loop < numToken; ++loop, IntPtr.Add(tmp, 1))
                        {
                            CXToken token = Marshal.PtrToStructure<CXToken>(tmp);
                            liter += clang.getTokenSpelling(ASTVisitor.CurrentTU, token).ToString();
                        }
                        clang.disposeTokens(ASTVisitor.CurrentTU, tokens, numToken);

                        // set default literal
                        param.DefaultValue = liter;
                    }
                    return CXChildVisitResult.CXChildVisit_Continue;
                }, new CXClientData(IntPtr.Zero));

                thisFunc.AddParameter(param);
            }

            return CXChildVisitResult.CXChildVisit_Recurse;
        }
    }
}
