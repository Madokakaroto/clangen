using System;
using System.Collections.Generic;
using System.Text;
using ClangSharp;

namespace clangen
{
    public class ClassTemplateVisitor : IASTVisitor
    {
        AST AST_;
        ASTVisitor visitor_;
        TemplateProto tp_;

        public ClassTemplateVisitor(AST ast, ASTVisitor visitor)
        {
            AST_ = ast;
            visitor_ = visitor;
        }

        public bool DoVisit(CXCursor cursor, CXCursor parent)
        {
            string displayName = clang.getCursorDisplayName(cursor).ToString();
            string namespaceName = visitor_.CurrentNamespace;

            tp_ = new TemplateProto();

            clang.visitChildren(cursor, Visitor, new CXClientData(IntPtr.Zero));
            return true;
        }

        private CXChildVisitResult Visitor(CXCursor cursor, CXCursor parent, IntPtr data)
        {
            CXCursorKind kind = cursor.kind;
            switch (kind)
            {
                case CXCursorKind.CXCursor_TemplateTypeParameter:
                    string paramName = clang.getCursorSpelling(cursor).ToString();
                    TemplateParameter param = new TemplateParameter(paramName);
                    tp_.AddTemplateParameter(param);
                    break;
                case CXCursorKind.CXCursor_NonTypeTemplateParameter:
                    CXType type = clang.getCursorType(cursor);
                    NativeType nativeType = TypeVisitor.GetNativeType(AST_, type);
                    break;
                case CXCursorKind.CXCursor_TemplateTemplateParameter:
                    return CXChildVisitResult.CXChildVisit_Recurse;
                default:
                    break;
            }
            return CXChildVisitResult.CXChildVisit_Continue;
        }
    }
}
