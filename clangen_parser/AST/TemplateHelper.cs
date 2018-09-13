using System;
using System.Diagnostics;
using System.Collections.Generic;
using ClangSharp;

namespace clangen
{
    public class TemplateHelper
    {
        public static bool VisitTemplate(CXCursor cursor, NativeClass c, AST ast)
        {
            CXType type = clang.getCursorType(cursor);
            CXCursor templateCursor = clang.getSpecializedCursorTemplate(cursor);
            if (ClangTraits.IsInvalid(templateCursor))
                return false;

            if(clang.isCursorDefinition(cursor) != 0)
            {
                // definition is a full specialization
                c.IsFullSpecialization = true;
            }
            string templateID = clang.getCursorUSR(templateCursor).ToString();
            ClassTemplate template = ast.GetClassTemplate(templateID);
            c.SetTemplate(template);
            return true;
        }

        public static bool VisitTemplateParameter(
            CXCursor cursor, 
            CXType type, 
            NativeClass @class, 
            AST ast,
            TypeVisitContext context)
        {
            ClassTemplate template = @class.InstanceOf;
            Debug.Assert(template != null);
            Debug.Assert(template.TP != null);
            Debug.Assert(context != null);

            //List<string> context = new List<string>();
            //clang.visitChildren(cursor, (CXCursor c, CXCursor p, IntPtr data) =>
            //{
            //    if(ClangTraits.IsNonTypeTemplateParamLiteral(c))
            //    {
            //        List<string> tokens = ASTVisitor.GetCursorTokens(c);
            //        context.Add(string.Concat(tokens));
            //    }
            //    else if(ClangTraits.IsTemplateRef(c))
            //    {
            //        CXCursor refCursor = clang.getCursorReferenced(c);
            //        string templateID = clang.getCursorUSR(refCursor).ToString();
            //        context.Add(templateID);
            //    }
            //    return CXChildVisitResult.CXChildVisit_Continue;
            //}, new CXClientData(IntPtr.Zero));

            int templateArgNum = clang.Type_getNumTemplateArguments(type);
            if (templateArgNum < 0)
                return false;

            @class.SetTemplateParameterCount(templateArgNum);
            int contextIndex = 0;
            for(uint loop = 0; loop < templateArgNum; ++loop)
            {
                TemplateParameter param = template.TP.GetTemplateParameter(loop);
                Debug.Assert(param != null);

                if(param.Kind == TemplateParameterKind.Type)
                {
                    CXType argType = clang.Type_getTemplateArgumentAsType(type, loop);
                    Debug.Assert(!ClangTraits.IsInvalid(argType));
                    NativeType nativeArgType = TypeVisitorHelper.GetNativeType(ast, argType, context);
                    @class.SetTemplateParameter(loop, nativeArgType);
                }
                else if(param.Kind == TemplateParameterKind.NoneType ||
                    param.Kind == TemplateParameterKind.Dependent)
                {
                    string literal = context.Consume();
                    @class.SetTemplateParameter(loop, literal);
                    ++contextIndex;
                }
                else
                {
                    Debug.Assert(TemplateParameterKind.Template == param.Kind);
                    string templateID = context.Consume();
                    ClassTemplate templateParam = ast.GetClassTemplate(templateID);
                    @class.SetTemplateParameter(loop, templateParam);
                }
            }

            return true;
        }
    }

}
