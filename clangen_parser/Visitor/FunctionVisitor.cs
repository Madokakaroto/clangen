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
                function.Parsed = true;

                // spelling as unscoped name
                function.UnscopedName = clang.getCursorSpelling(cursor).ToString();

                // create proto
                FunctionProto proto = new FunctionProto();

                // proces result type
                CXType resultType = clang.getCursorResultType(cursor);
                proto.ResultType = TypeVisitorHelper.GetNativeType(AST_, resultType);

                // create IntPtr for context
                GCHandle protoHandle = GCHandle.Alloc(proto);

                // visit children
                clang.visitChildren(cursor, Visitor, new CXClientData((IntPtr)protoHandle));

                // set proto
                function.Proto = proto;
            }

            return true;
        }

        private CXChildVisitResult Visitor(CXCursor cursor, CXCursor parent, IntPtr data)
        {
            if(CXCursorKind.CXCursor_ParmDecl == cursor.kind)
            {
                // prepare client data
                GCHandle funcHandle = (GCHandle)data;
                FunctionProto proto = funcHandle.Target as FunctionProto;

                CXType type = clang.getCursorType(cursor);

                FunctionParameter param = new FunctionParameter
                {
                    Name = clang.getCursorSpelling(cursor).ToString(),
                    Type = TypeVisitorHelper.GetNativeType(AST_, type)
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

                proto.AddParameter(param);
            }

            return CXChildVisitResult.CXChildVisit_Recurse;
        }
    }
}
