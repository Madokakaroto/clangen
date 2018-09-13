using System.Diagnostics;
using System.Collections.Generic;
using ClangSharp;

namespace clangen
{
    public class TemplateHelper
    {
        public static bool VisitTemplateArguments(CXCursor cursor, NativeClass c, AST ast)
        {
            CXType type = clang.getCursorType(cursor);
            int templateNum = clang.Type_getNumTemplateArguments(type);
            if(templateNum > 0)
            {
                CXCursor templateCursor = clang.getSpecializedCursorTemplate(cursor);
                string templateID = clang.getCursorUSR(templateCursor).ToString();
                ClassTemplate tempplate = ast.GetClassTemplate(templateID);
                c.SetTemplate(tempplate);
                c.SetTemplateParameterCount(templateNum);
                for(int loop = 0; loop < templateNum; ++loop)
                {

                }
                return true;
            }
            return false;
        }
    }

}
