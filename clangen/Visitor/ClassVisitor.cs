using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ClangSharp;

namespace clangen
{
    class ClassVisitor : IASTVisitor
    {
        private AST AST_;
        private NativeClass OwnerClass_;

        public ClassVisitor(AST ast, NativeClass owner = null)
        {
            AST_ = ast;
            OwnerClass_ = owner;
        }

        public bool DoVisit(CXCursor cursor, CXCursor parent)
        {
            // get class type
            CXType type = clang.getCursorType(cursor);

            // get class name
            string className = clang.getTypeSpelling(type).ToString();

            // get class object
            NativeClass @class = AST_.GetClass(className);

            // is definition
            bool isDefinition = clang.isCursorDefinition(cursor) != 0;

            // dealing with defition and unsettled types
            if (!@class.Parsed && isDefinition)
            {
                //proces class detail
                ProcessClassDetail(@class, cursor, type, parent);

                // create IntPtr for context
                GCHandle classHandle = GCHandle.Alloc(@class);

                // visit children
                clang.visitChildren(cursor, Visitor, new CXClientData((IntPtr)classHandle));

                @class.Parsed = true;
            }

            return true;
        }

        private CXChildVisitResult Visitor(CXCursor cursor, CXCursor parent, IntPtr data)
        {
            // prepare client data
            GCHandle classHandle = (GCHandle)data;
            NativeClass thisClass = classHandle.Target as NativeClass;

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
                case CXCursorKind.CXCursor_StructDecl:
                case CXCursorKind.CXCursor_ClassDecl:
                    // recursive visit child
                    ClassVisitor subClassVisitor = new ClassVisitor(AST_, thisClass);
                    subClassVisitor.DoVisit(cursor, parent);
                    break;
                case CXCursorKind.CXCursor_TypedefDecl:
                case CXCursorKind.CXCursor_TypeAliasDecl:
                    ProcessTypeExport(thisClass, cursor, parent);
                    break;
                case CXCursorKind.CXCursor_ClassTemplate:
                case CXCursorKind.CXCursor_ClassTemplatePartialSpecialization:
                    break;
                case CXCursorKind.CXCursor_FunctionTemplate:
                    break;
                default:
                    break;
            }

            return CXChildVisitResult.CXChildVisit_Continue;
        }

        private void ProcessBaseClass(NativeClass thisClass, CXCursor cursor, CXCursor parent)
        {
            // get class name
            CXType type = clang.getCursorType(cursor);
            CXCursor typeDeclCursor = clang.getTypeDeclaration(type);
            string name = clang.getTypeSpelling(type).ToString();

            // create the base class
            BaseClass baseClass = new BaseClass
            {
                // check access specifier
                Access = ClangTraits.ToAccessSpecifier(clang.getCXXAccessSpecifier(cursor)),
                // native class type
                Class = AST_.GetClass(name),
                // check is virtual base
                IsVirtual = clang.CXXRecord_isAbstract(typeDeclCursor) != 0
            };

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

            // create method
            Method memberFunc = new Method( thisClass,
                name, isStataic, isConst, isVirtual, isAbastrct);

            // proces result type
            CXType resultType = clang.getCursorResultType(cursor);
            memberFunc.ResultType = TypeVisitor.GetNativeType(AST_, resultType);

            // deep visit children
            GCHandle astHandle = GCHandle.Alloc(memberFunc);
            clang.visitChildren(cursor, MethodChildVisitor, new CXClientData((IntPtr)astHandle));

            // register method
            thisClass.AddMethod(memberFunc);
        }

        private CXChildVisitResult MethodChildVisitor(CXCursor cursor, CXCursor parent, IntPtr data)
        {
            if(CXCursorKind.CXCursor_ParmDecl == cursor.kind)
            {
                // prepare client data
                GCHandle astHandle = (GCHandle)data;
                Method thisMethod = astHandle.Target as Method;

                CXType type = clang.getCursorType(cursor);

                FunctionParameter param = new FunctionParameter
                {
                    Name = clang.getCursorSpelling(cursor).ToString(),
                    Type = TypeVisitor.GetNativeType(AST_, type)
                };

                clang.visitChildren(cursor, (CXCursor c, CXCursor p, IntPtr d) => 
                {
                    if(ClangTraits.IsLiteralCursor(c))
                    {
                        // get liter-string from token
                        CXSourceRange range = clang.getCursorExtent(c);
                        IntPtr tokens;
                        uint numToken;
                        string liter = "";
                        clang.tokenize(ASTVisitor.CurrentTU, range, out tokens, out numToken);
                        IntPtr tmp = tokens;
                        for (uint loop = 0; loop < numToken; ++loop, IntPtr.Add(tmp, 1))
                        {
                            CXToken token = Marshal.PtrToStructure<CXToken>(tmp);
                            liter += clang.getTokenSpelling(ASTVisitor.CurrentTU, token).ToString();
                        }
                        clang.disposeTokens(ASTVisitor.CurrentTU, tokens, numToken);

                        // set default literal
                        param.DefaultValue = liter;
                    }
                    return CXChildVisitResult.CXChildVisit_Continue;
                }, new CXClientData(IntPtr.Zero));

                thisMethod.AddParameter(param);
            }

            return CXChildVisitResult.CXChildVisit_Recurse;
        }

        private void ProcessClassDetail(
            NativeClass thisClass,              // this class to parse
            CXCursor cursor,                    // current cursor
            CXType type,                        // current cursor type
            CXCursor parent                     // parent cursor
            )
        {
            // set struct or class
            thisClass.ClassTag = ClangTraits.ToStructOrClass(cursor.kind);

            // abstract
            thisClass.IsAbstract = clang.CXXRecord_isAbstract(cursor) != 0;

            // virtual base
            thisClass.IsVirtualBase = clang.isVirtualBase(cursor) != 0;

            // set template instance info
            int templateNum = clang.Type_getNumTemplateArguments(type);
            if(templateNum > 0)
            {
                thisClass.SetTemplateParameterCount((uint)templateNum);
                for (int loop = 0; loop < templateNum; ++loop)
                {
                    CXType argType = clang.Type_getTemplateArgumentAsType(type, (uint)loop);
                    NativeType nativeType = TypeVisitor.GetNativeType(AST_, argType);
                    thisClass.SetTemplateParameter((uint)loop, nativeType);
                }
            }

            // set subclass
            if(ClangTraits.IsUserDefinedTypeDecl(parent))
            {
                Debug.Assert(OwnerClass_ != null);
                thisClass.IsEmbedded = true;
                thisClass.OwnerClass = OwnerClass_;

                SubClass subClass = new SubClass
                {
                    Access = ClangTraits.ToAccessSpecifier(clang.getCXXAccessSpecifier(cursor)),
                    Class = thisClass
                };

                OwnerClass_.AddSubClass(subClass);
            }
        }

        private void ProcessField(NativeClass thisClass, CXCursor cursor, CXCursor parent, bool isStatic = false)
        {
            // get field name
            string fieldName = clang.getCursorSpelling(cursor).ToString();

            // get field type
            CXType type = clang.getCursorType(cursor);
            NativeType nativeType = TypeVisitor.GetNativeType(AST_, type);

            // get field access specifier
            AccessSpecifier access = ClangTraits.ToAccessSpecifier(clang.getCXXAccessSpecifier(cursor));

            // create field object
            Field f = new Field
            {
                Access = access,
                Type = nativeType,
                IsStatic = isStatic
            };

            thisClass.AddField(fieldName, f);
        }

        private void ProcessTypeExport(NativeClass thisClass, CXCursor cursor, CXCursor parent)
        {
            // get field type
            CXType type = clang.getCursorType(cursor);
            NativeType nativeType = TypeVisitor.GetNativeType(AST_, type);

            // get field access specifier
            AccessSpecifier access = ClangTraits.ToAccessSpecifier(clang.getCXXAccessSpecifier(cursor));

            // create the exported member type
            MemberType memberType = new MemberType
            {
                Access = access,
                Type = nativeType
            };

            thisClass.AddMemberType(memberType);
        }
    }
}
