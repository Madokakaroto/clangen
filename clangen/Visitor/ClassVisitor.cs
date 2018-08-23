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
                case CXCursorKind.CXCursor_FieldDecl:
                    ProcessField(thisClass, cursor, parent);
                    break;
                case CXCursorKind.CXCursor_VarDecl:
                    ProcessField(thisClass, cursor, parent, true);
                    break;
                case CXCursorKind.CXCursor_CXXMethod:
                    ProcessMethod(thisClass, cursor, parent);
                    break;
                case CXCursorKind.CXCursor_CXXBaseSpecifier:
                    ProcessBaseClass(thisClass, cursor, parent);
                    break;
                case CXCursorKind.CXCursor_CXXFinalAttr:
                    thisClass.IsFinal = true;
                    break;
                default:
                    break;
            }

            return CXChildVisitResult.CXChildVisit_Continue;
        }

        private void ProcessBaseClass(NativeClass thisClass, CXCursor cursor, CXCursor parent)
        {
            BaseClass baseClass = new BaseClass();

            // get class
            CXType type = clang.getCursorType(cursor);
            CXCursor typeDeclCursor = clang.getTypeDeclaration(type);
            string name = clang.getTypeSpelling(type).ToString();
            baseClass.Class = AST_.GetClass(name);
            
            // check is virtual base
            baseClass.IsVirtual = clang.CXXRecord_isAbstract(typeDeclCursor) != 0;

            // check access specifier
            baseClass.Access = ClangTraits.ToAccessSpecifier(clang.getCXXAccessSpecifier(cursor));

            // register base class
            thisClass.AddBaseClass(baseClass);
        }

        private void ProcessMethod(NativeClass thisClass, CXCursor cursor, CXCursor parent)
        {
            string name = clang.getCursorSpelling(cursor).ToString();
            bool isStataic = clang.CXXMethod_isStatic(cursor) != 0;
            bool isConst = clang.CXXMethod_isConst(cursor) != 0;
            bool isVirtual = clang.CXXMethod_isVirtual(cursor) != 0;
            bool isAbastrct = clang.CXXMethod_isPureVirtual(cursor) != 0;

            Method memberFunc = new Method( thisClass,
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

            // abstract
            thisClass.IsAbstract = clang.CXXRecord_isAbstract(cursor) != 0;

            // virtual base
            thisClass.IsVirtualBae = clang.isVirtualBase(cursor) != 0;

            // set template instance info
            int templateNum = clang.Type_getNumTemplateArguments(type);
            if(templateNum > 0)
            {
                thisClass.SetTemplateParameterCount((uint)templateNum);
                for (int loop = 0; loop < templateNum; ++loop)
                {
                    CXType argType = clang.Type_getTemplateArgumentAsType(type, (uint)loop);
                    NativeType nativeType = TypeVisitHelper.GetNativeType(AST_, argType);
                    thisClass.SetTemplateParameter((uint)loop, nativeType);
                }
            }
        }

        private void ProcessField(NativeClass thisClass, CXCursor cursor, CXCursor parent, bool isStatic = false)
        {
            // get field name
            string fieldName = clang.getCursorSpelling(cursor).ToString();

            // get field type
            CXType type = clang.getCursorType(cursor);
            NativeType nativeType = TypeVisitHelper.GetNativeType(AST_, type);

            // get field access specifier
            AccessSpecifier access = ClangTraits.ToAccessSpecifier(clang.getCXXAccessSpecifier(cursor));

            // create field object
            Field f = new Field();
            f.Access = access;
            f.Type = nativeType;
            f.IsStatic = isStatic;

            thisClass.AddField(fieldName, f);
        }
    }
}
