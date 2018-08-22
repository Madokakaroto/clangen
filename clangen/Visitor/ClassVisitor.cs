using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ClangSharp;

namespace clangen
{
    class ClassVisitor : IASTVisitor
    {
        private AST AST_;

        public ClassVisitor(AST ast)
        {
            AST_ = ast;
        }

        public bool DoVisit(CXCursor cursor, CXCursor parent)
        {
            // get class token name
            string className = clang.getCursorDisplayName(cursor).ToString();

            // get class object
            bool unsetteld = false;
            NativeClass @class = AST_.GetClass(className, out unsetteld);
            if (unsetteld)
            {
                //proces class detail
                ProcessClassDetail(@class, cursor);

                // process parent
                //ProcessParent(@class, parent);

                // create IntPtr for context
                GCHandle astHandle = GCHandle.Alloc(@class);

                // visit children
                clang.visitChildren(cursor, Visitor, new CXClientData((IntPtr)astHandle));

                // add class
                AST_.AddClass(@class);
            }

            return true;
        }

        private CXChildVisitResult Visitor(CXCursor cursor, CXCursor parent, IntPtr data)
        {
            // prepare client data
            GCHandle astHandle = (GCHandle)data;
            NativeClass thisClass = astHandle.Target as NativeClass;

            CXCursorKind kind = clang.getCursorKind(cursor);
            switch(kind)
            {
                case CXCursorKind.CXCursor_CXXMethod:
                    ProcessMethod(thisClass, cursor, parent);
                    break;
                case CXCursorKind.CXCursor_FieldDecl:
                    break;
                case CXCursorKind.CXCursor_CXXBaseSpecifier:
                    ProcessBaseClass(thisClass, cursor, parent);
                    break;
                default:
                    break;
            }

            return CXChildVisitResult.CXChildVisit_Continue;
        }

        private void ProcessBaseClass(NativeClass thisClass, CXCursor cursor, CXCursor parent)
        {
            CXCursor refCursor = clang.getCursorReferenced(cursor);
            string name = clang.getCursorSpelling(refCursor).ToString();
            NativeClass @class = AST_.GetClass(name);
            thisClass.AddBaseClass(@class);
        }

        private void ProcessMethod(NativeClass thisClass, CXCursor cursor, CXCursor parent)
        {
            string name = clang.getCursorSpelling(cursor).ToString();
            bool isStataic = clang.CXXMethod_isStatic(cursor) != 0;
            bool isConst = clang.CXXMethod_isConst(cursor) != 0;
            bool isVirtual = clang.CXXMethod_isVirtual(cursor) != 0;
            bool isAbastrct = clang.CXXMethod_isPureVirtual(cursor) != 0;

            MemberFunction memberFunc = new MemberFunction( thisClass,
                name, isStataic, isConst, isVirtual, isAbastrct);

            thisClass.AddMethod(memberFunc);

            // TODO deep iterate
        }

        private bool NestedClassCursor(CXCursor parent)
        {
            //return (parent.kind == CXCursorKind.CXCursor_ClassDecl) ||
            //    (parent.kind == CXCursorKind.CXCursor_StructDecl) ||
            //    /*(parent.kind == CXCursorKind.CXCursor_ClassTemplate) ||*/
            //    (parent.kind == CXCursorKind.CXCursor_ClassTemplatePartialSpecialization);
            if (parent.kind == CXCursorKind.CXCursor_ClassDecl || parent.kind == CXCursorKind.CXCursor_StructDecl)
                return true;

            return false;
        }

        private void ProcessClassDetail(NativeClass thisClass, CXCursor cursor)
        {
            // get cursor kind
            CXCursorKind cursorKind = clang.getCursorKind(cursor);
            Debug.Assert(
                cursorKind == CXCursorKind.CXCursor_ClassDecl ||
                cursorKind == CXCursorKind.CXCursor_StructDecl);

            // set struct or class
            thisClass.ClassTag = cursorKind == CXCursorKind.CXCursor_ClassDecl ?
                StructOrClass.Class : StructOrClass.Struct;

            // set template instance info
            CXType theType = clang.getCursorType(cursor);
            int templateNum = clang.Type_getNumTemplateArguments(theType);
            
            if(templateNum < 0)
            {
                thisClass.IsTemplateInstance = false;
            }
            else
            {
                thisClass.IsTemplateInstance = true;
                for(int loop = 0; loop < templateNum; ++loop)
                {
                    CXType type = clang.Type_getTemplateArgumentAsType(theType, (uint)loop);

                    // TODO ... add to this class
                }
            }
        }

        private void ProcessParent(NativeClass thisClass, CXCursor parent)
        {
            //if(parent.kind )
            CXCursor temp = parent;
            if(NestedClassCursor(parent))
            {
                //string ownerClassName = 
            }
            else
            {
                while (temp.kind != CXCursorKind.CXCursor_TranslationUnit)
                {
                    if (temp.kind == CXCursorKind.CXCursor_Namespace)
                    {

                    }

                }
            }
        }
    }
}
