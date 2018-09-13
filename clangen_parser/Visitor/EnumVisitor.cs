using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ClangSharp;

namespace clangen
{
    class EnumVisitor : IASTVisitor
    {
        private AST AST_;
        private NativeClass OwnerClass_;

        public EnumVisitor(AST ast, NativeClass ownerClass = null)
        {
            AST_ = ast;
            OwnerClass_ = ownerClass;
        }

        public bool DoVisit(CXCursor cursor, CXCursor parent)
        {
            //string name = clang.getCursorSpelling(cursor).ToString();
            string name = clang.getTypeSpelling(clang.getCursorType(cursor)).ToString();

            Enumeration @enum = AST_.GetEnum(name);
            if(!@enum.Parsed)
            {
                @enum.Parsed = true;

                ProcessEnumDetail(@enum, cursor, parent);

                // create IntPtr for context
                GCHandle enumHandle = GCHandle.Alloc(@enum);

                 // visit children
                clang.visitChildren(cursor, Visitor, new CXClientData((IntPtr)enumHandle));
            }

            return true;
        }

        private CXChildVisitResult Visitor(CXCursor cursor, CXCursor parent, IntPtr data)
        {
            if(cursor.kind == CXCursorKind.CXCursor_EnumConstantDecl)
            {
                // prepare enumeration
                  // prepare client data
                GCHandle enumHandle = (GCHandle)data;
                Enumeration thisEnum = enumHandle.Target as Enumeration;

                // get constant name
                string constantName = clang.getCursorSpelling(cursor).ToString();

                // get enum constant value
                EnumField c = new EnumField { Name = constantName };
                CXType underlyingType = clang.getEnumDeclIntegerType(parent);
                NativeType underlyingNativeType = TypeVisitorHelper.GetNativeType(AST_, underlyingType);
                if (ASTTraits.IsSigned(underlyingNativeType))
                {
                    c.Constant = clang.getEnumConstantDeclValue(cursor);
                }
                else
                {
                    Debug.Assert(ASTTraits.IsUnSigned(underlyingNativeType));
                    c.Constant = (long)clang.getEnumConstantDeclUnsignedValue(cursor);
                }

                // add it to enumeration
                thisEnum.AddConstant(c);
            }
            return CXChildVisitResult.CXChildVisit_Continue;
        }

        void ProcessEnumDetail(Enumeration @enum, CXCursor cursor, CXCursor parent)
        {
            // unscoped name
            @enum.UnscopedName = clang.getCursorSpelling(cursor).ToString();

            // underlying type
            CXType underlyingType = clang.getEnumDeclIntegerType(cursor);
            BasicType type = ClangTraits.ToBasicType(underlyingType);
            @enum.Type = type;

            // is scoped
            @enum.IsEnumClass = clang.EnumDecl_isScoped(cursor) != 0;

            // check is parent is a class
            if(OwnerClass_ != null)
            {
                Debug.Assert(ClangTraits.IsUserDefinedTypeDecl(parent));
                OwnerClass_.AddSubEnum(new SubEnum
                {
                    Access = ClangTraits.ToAccessSpecifier(clang.getCXXAccessSpecifier(cursor)),
                    Enum = @enum
                });
            }
        }
    }
}
