using System.Diagnostics;
using ClangSharp;

namespace clangen
{
    public class ClangTraits
    {
        public static bool IsInvalid(CXType type)
        {
            return type.kind == CXTypeKind.CXType_Invalid;
        }

        public static bool IsVoid(CXType type)
        {
            return type.kind == CXTypeKind.CXType_Void;
        }

        public static bool IsPointer(CXType type)
        {
            return type.kind == CXTypeKind.CXType_Pointer;
        }

        public static bool IsLValueReference(CXType type)
        {
            return type.kind == CXTypeKind.CXType_LValueReference;
        }

        public static bool IsRValueReference(CXType type)
        {
            return type.kind == CXTypeKind.CXType_RValueReference;
        }

        public static bool IsReference(CXType type)
        {
            return IsRValueReference(type) || IsRValueReference(type);
        }

        public static bool IsConst(CXType type)
        {
            return clang.isConstQualifiedType(type) != 0;
        }

        public static bool IsIntegral(CXType type)
        {
            if (type.kind <= CXTypeKind.CXType_Int128 &&
               type.kind >= CXTypeKind.CXType_Bool)
                return true;
            return false;
        }

        public static bool IsSigned(CXType type)
        {
            return type.kind >= CXTypeKind.CXType_Char_S &&
                type.kind <= CXTypeKind.CXType_Int128;
        }

        public static bool IsUnsigned(CXType type)
        {
             return type.kind >= CXTypeKind.CXType_Char_U &&
                type.kind <= CXTypeKind.CXType_UInt128;
        }

        public static bool IsFloatingPoint(CXType type)
        {
            if (type.kind >= CXTypeKind.CXType_Float &&
               type.kind <= CXTypeKind.CXType_LongDouble)
                return true;
            return false;
        }

        public static bool IsArithmetic(CXType type)
        {
            return IsIntegral(type) || IsFloatingPoint(type);
        }

        public static bool IsBuiltInType(CXType type)
        {
            return (type.kind >= CXTypeKind.CXType_FirstBuiltin) &&
                (type.kind <= CXTypeKind.CXType_LastBuiltin);
        }

        public static bool IsEnum(CXType type)
        {
            return type.kind == CXTypeKind.CXType_Enum;
        }

        public static bool IsUserDefiendType(CXType type)
        {
            return type.kind == CXTypeKind.CXType_Record;
        }

        public static bool IsUnexposedType(CXType type)
        {
            return type.kind == CXTypeKind.CXType_Unexposed;
        }

        public static bool IsTerminalType(CXType type)
        {
            return IsBuiltInType(type) || 
                   IsEnum(type) || 
                   IsUserDefiendType(type) ||
                   IsUnexposedType(type);
        }

        public static bool IsTypedef(CXType type)
        {
            return type.kind == CXTypeKind.CXType_Typedef;
        }

        public static BasicType ToBasicType(CXType type)
        {
            // TODO ... complete
            switch (type.kind)
            {
                case CXTypeKind.CXType_Void:
                    return BasicType.Void;
                case CXTypeKind.CXType_Bool:
                    return BasicType.Bool;
                case CXTypeKind.CXType_Char_U:
                case CXTypeKind.CXType_UChar:
                    return BasicType.UChar;
                case CXTypeKind.CXType_Char16:
                    return BasicType.Int16;
                case CXTypeKind.CXType_Char32:
                    return BasicType.Int32;
                case CXTypeKind.CXType_UShort:
                    return BasicType.UInt16;
                case CXTypeKind.CXType_UInt:
                    return BasicType.UInt32;
                case CXTypeKind.CXType_ULong:
                    return BasicType.UInt64;
                case CXTypeKind.CXType_Char_S:
                case CXTypeKind.CXType_SChar:
                    return BasicType.Char;
                case CXTypeKind.CXType_WChar:
                case CXTypeKind.CXType_Short:
                    return BasicType.Int16;
                case CXTypeKind.CXType_Int:
                    return BasicType.Int32;
                case CXTypeKind.CXType_Long:
                    return BasicType.Int64;
                case CXTypeKind.CXType_Float:
                    return BasicType.Float;
                case CXTypeKind.CXType_Double:
                    return BasicType.Double;
                case CXTypeKind.CXType_LongDouble:
                    return BasicType.LongDouble;
                case CXTypeKind.CXType_Record:
                    return BasicType.Object;
                case CXTypeKind.CXType_Enum:
                    return BasicType.Enum;
                default:
                    return BasicType.Unknown;
            }
        }

        public static AccessSpecifier ToAccessSpecifier(CX_CXXAccessSpecifier accessSpecifier)
        {
            return (AccessSpecifier)(accessSpecifier);
        }

        public static bool IsLiteralCursor(CXCursor cursor)
        {
            return cursor.kind >= CXCursorKind.CXCursor_IntegerLiteral &&
                cursor.kind <= CXCursorKind.CXCursor_CharacterLiteral;
        }

        public static bool IsUserDefinedTypeDecl(CXCursor cursor)
        {
            return cursor.kind == CXCursorKind.CXCursor_ClassDecl ||
                cursor.kind == CXCursorKind.CXCursor_StructDecl;
        }

        public static bool IsUserDefinedTypeDecl(CXCursorKind cursorKind)
        {
            return cursorKind == CXCursorKind.CXCursor_ClassDecl ||
                cursorKind == CXCursorKind.CXCursor_StructDecl;
        }

        public static StructOrClass ToStructOrClass(CXCursorKind kind)
        {
            Debug.Assert(IsUserDefinedTypeDecl(kind));
            return kind == CXCursorKind.CXCursor_ClassDecl ?
                StructOrClass.Class : StructOrClass.Struct;
        }
    }
}
