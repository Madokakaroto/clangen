using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ClangSharp;

namespace clangen
{
    class ClassTemplateVisitor : IASTVisitor
    {
        AST AST_;
        ASTVisitor visitor_;
        bool isPartial_;

        public ClassTemplateVisitor(AST ast, ASTVisitor visitor, bool isPartial)
        {
            AST_ = ast;
            visitor_ = visitor;
            isPartial_ = isPartial;
        }

        public bool DoVisit(CXCursor cursor, CXCursor parent)
        {
            string id = clang.getCursorUSR(cursor).ToString();
            ClassTemplate template = AST_.GetClassTemplate(id);

            bool isDefinition = clang.isCursorDefinition(cursor) != 0;
            if (!template.Parsed)
            {
                if(template.TP == null)
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
                }

                // do parse for template definition
                if(isDefinition)
                {
                    template.Parsed = true;
                    GCHandle classTemplateHandle = GCHandle.Alloc(template);
                    clang.visitChildren(cursor, Visitor, new CXClientData((IntPtr)classTemplateHandle));
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
                        tp.AddTemplateParameter(GetTemplateNonTypeParameter(AST_, tp, c));
                        break;
                    case CXCursorKind.CXCursor_TemplateTemplateParameter:
                        tp.AddTemplateParameter(GetTemplateTemplateParameter(c));
                        break;
                    default:
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
            TemplateParameter param = new TemplateParameter(paramName, TemplateParameterKind.Type, isVariadic);
            return param;
        }

        private TemplateParameter GetTemplateNonTypeParameter(AST ast, TemplateProto tp, CXCursor cursor)
        {
            string paramName = clang.getCursorSpelling(cursor).ToString();
            bool isVariadic = ClangTraits.IsVariadicTemplateParameter(cursor);

            // check if dependent or nontype
            bool isDependent = false;
            string dependName = null;
            clang.visitChildren(cursor, (CXCursor c, CXCursor p, IntPtr data) =>
            {
                if(CXCursorKind.CXCursor_TypeRef == c.kind)
                {
                    isDependent = true;
                    dependName = clang.getCursorSpelling(c).ToString();
                }
                return CXChildVisitResult.CXChildVisit_Continue;
            }, new CXClientData(IntPtr.Zero));

            TemplateParameter param;
            if (isDependent)
            {
                Debug.Assert(dependName != null);
                param = new TemplateParameter(paramName, TemplateParameterKind.Dependent, isVariadic);
                TemplateParameter dependeParam = tp.GetTemplateParameter(dependName);
                Debug.Assert(dependeParam != null);
                param.SetExtra(dependeParam);
            }
            else
            {
                CXType type = clang.getCursorType(cursor);
                NativeType nativeType = TypeVisitorHelper.GetNativeType(AST_, type);
                param = new TemplateParameter(paramName, TemplateParameterKind.NoneType, isVariadic);
                param.SetExtra(nativeType);
            }
            return param;
        }

        private TemplateParameter GetTemplateTemplateParameter(CXCursor cursor)
        {
            string paramName = clang.getCursorSpelling(cursor).ToString();
            bool isVariadic = ClangTraits.IsVariadicTemplateParameter(cursor);
            TemplateParameter param = new TemplateParameter(paramName, TemplateParameterKind.Template, isVariadic);
            TemplateProto proto = GetTemplateProto(cursor);
            param.SetExtra(proto);
            return param;
        }
    }
}
