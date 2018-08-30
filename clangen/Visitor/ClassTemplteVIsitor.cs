using System;
using System.Runtime.InteropServices;
using ClangSharp;

namespace clangen
{
    public class ClassTemplateVisitor : IASTVisitor
    {
        AST AST_;
        ASTVisitor visitor_;

        public ClassTemplateVisitor(AST ast, ASTVisitor visitor)
        {
            AST_ = ast;
            visitor_ = visitor;
        }

        public bool DoVisit(CXCursor cursor, CXCursor parent)
        {
            string name = visitor_.GetCurrentScopeName(clang.getCursorDisplayName(cursor).ToString());
            TemplateProto tp = GetTemplateProto(cursor);
            ClassTemplate template = AST_.GetClassTemplate(name, tp);
            if(!template.Parsed)
            {
                template.Spelling = visitor_.GetCurrentScopeName(
                    clang.getCursorSpelling(cursor).ToString());
                if(clang.isCursorDefinition(cursor) != 0)
                {
                    GCHandle classTemplateHandle = GCHandle.Alloc(template);
                    clang.visitChildren(cursor, Visitor, new CXClientData((IntPtr)classTemplateHandle));
                    template.Parsed = true;
                }
            }
            return true;
        }

        private CXChildVisitResult Visitor(CXCursor cursor, CXCursor parent, IntPtr data)
        {
            return CXChildVisitResult.CXChildVisit_Continue;
        }

        private TemplateProto GetTemplateProto(CXCursor cursor)
        {
            TemplateProto tp = new TemplateProto();
            clang.visitChildren(cursor, (CXCursor c, CXCursor p, IntPtr data) =>
            {
                switch (c.kind)
                {
                    case CXCursorKind.CXCursor_TemplateTypeParameter:
                        tp.AddTemplateParameter(GetTemplateTypeParameter(c));
                        break;
                    case CXCursorKind.CXCursor_NonTypeTemplateParameter:
                        tp.AddTemplateParameter(GetTemplateNonTypeParameter(AST_, c));
                        break;
                    case CXCursorKind.CXCursor_TemplateTemplateParameter:
                        tp.AddTemplateParameter(GetTemplateTemplateParameter(c));
                        break;
                }
                return CXChildVisitResult.CXChildVisit_Continue;
            }, new CXClientData(IntPtr.Zero));
            return tp;
        }

        private TemplateParameter GetTemplateTypeParameter(CXCursor cursor)
        {
            string paramName = clang.getCursorSpelling(cursor).ToString();
            bool isVariadic = ClangTraits.IsVariadicTemplateParameter(cursor);
            TemplateParameter param = new TemplateParameter(paramName, isVariadic);
            return param;
        }

        private TemplateParameter GetTemplateNonTypeParameter(AST ast, CXCursor cursor)
        {
            string paramName = clang.getCursorSpelling(cursor).ToString();
            bool isVariadic = ClangTraits.IsVariadicTemplateParameter(cursor);
            TemplateParameter param = new TemplateParameter(paramName, isVariadic);
            CXType type = clang.getCursorType(cursor);
            NativeType nativeType = TypeVisitor.GetNativeType(AST_, type);
            param.Set(nativeType);
            return param;
        }

        private TemplateParameter GetTemplateTemplateParameter(CXCursor cursor)
        {
            string paramName = clang.getCursorSpelling(cursor).ToString();
            bool isVariadic = ClangTraits.IsVariadicTemplateParameter(cursor);
            TemplateParameter param = new TemplateParameter(paramName, isVariadic);
            TemplateProto proto = GetTemplateProto(cursor);
            param.Set(proto);
            return param;
        }
    }
}
