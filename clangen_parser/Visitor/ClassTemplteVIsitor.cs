using System;
using System.Runtime.InteropServices;
using ClangSharp;

namespace clangen
{
    class ClassTemplateVisitor : IASTVisitor
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
            string id = clang.getCursorUSR(cursor).ToString();
            ClassTemplate template = AST_.GetClassTemplate(id);
            bool isDefinition = clang.isCursorDefinition(cursor) != 0;
            if (!template.Parsed && isDefinition)
            {
                // proto
                template.TP = GetTemplateProto(cursor);

                // name
                template.Name = visitor_.GetCurrentScopeName(
                    clang.getCursorDisplayName(cursor).ToString()
                    );

                // spelling
                template.Spelling = visitor_.GetCurrentScopeName(
                    clang.getCursorSpelling(cursor).ToString()
                    );

                // do parse for template definition
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
            switch (cursor.kind)
            {
                case CXCursorKind.CXCursor_TypedefDecl:
                case CXCursorKind.CXCursor_TypeAliasDecl:
                    break;
                case CXCursorKind.CXCursor_FieldDecl:
                    //CXType type = clang.getCursorType(cursor);
                    //CXType type1 = clang.getPointeeType(type);
                    //
                    //CXCursor c = clang.getTypeDeclaration(type);
                    //CXCursor templateCursor = clang.getSpecializedCursorTemplate(cursor);
                    break;
                case CXCursorKind.CXCursor_CXXMethod:
                    break;
            }
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
