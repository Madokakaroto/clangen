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
            // get class type
            CXType type = clang.getCursorType(cursor);

            // get class name
            string className = clang.getTypeSpelling(type).ToString();

            // get class object
            bool unsetteld = false;
            NativeClass @class = AST_.GetClass(className, out unsetteld);
            if (unsetteld)
            {
                //proces class detail
                ProcessClassDetail(@class, cursor, type);

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
            CXType type = clang.getCursorType(cursor);
            string name = clang.getTypeSpelling(type).ToString();
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

        private void ProcessClassDetail(
            NativeClass thisClass,              // this class to parse
            CXCursor cursor,                    // current cursor
            CXType type                         // current cursor type
            )
        {
            // check cursor kind
            CXCursorKind cursorKind = cursor.kind;
            Debug.Assert(
                cursorKind == CXCursorKind.CXCursor_ClassDecl ||
                cursorKind == CXCursorKind.CXCursor_StructDecl);

            // set struct or class
            thisClass.ClassTag = cursorKind == CXCursorKind.CXCursor_ClassDecl ?
                StructOrClass.Class : StructOrClass.Struct;

            // set template instance info
            int templateNum = clang.Type_getNumTemplateArguments(type);
            if(templateNum < 0)
            {
                thisClass.IsTemplateInstance = false;
            }
            else
            {
                thisClass.IsTemplateInstance = true;
                for(int loop = 0; loop < templateNum; ++loop)
                {
                    CXType argType = clang.Type_getTemplateArgumentAsType(type, (uint)loop);

                    // TODO ... add to this class
                    TypeVisitor.CreateNativeType(AST_, argType);
                }
            }
        }


    }
}
