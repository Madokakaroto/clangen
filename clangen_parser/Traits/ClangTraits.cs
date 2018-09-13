using System.Collections.Generic;
using System.Diagnostics;
using ClangSharp;

namespace clangen
{
    class ClangTraits
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

        public static bool IsElaboratedType(CXType type)
        {
            return type.kind == CXTypeKind.CXType_Elaborated;
        }

        public static bool IsFunction(CXType type)
        {
            return type.kind == CXTypeKind.CXType_FunctionProto;
        }

        public static bool IsArray(CXType type)
        {
            return type.kind == CXTypeKind.CXType_ConstantArray || 
                type.kind == CXTypeKind.CXType_IncompleteArray || 
                type.kind == CXTypeKind.CXType_DependentSizedArray;
        }

        public static bool IsIncompleteArray(CXType type)
        {
            return type.kind == CXTypeKind.CXType_IncompleteArray;
        }

        public static bool IsTypeEntity(CXType type)
        {
            return IsBuiltInType(type) || 
                   IsEnum(type) || 
                   IsUserDefiendType(type) ||
                   IsUnexposedType(type) ||
                   IsElaboratedType(type) ||
                   IsFunction(type);
        }

        public static bool IsTypedef(CXType type)
        {
            return type.kind == CXTypeKind.CXType_Typedef;
        }

        public static bool IsMemberPointer(CXType type)
        {
            return type.kind == CXTypeKind.CXType_MemberPointer;
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
                case CXTypeKind.CXType_ULong:
                    return BasicType.UInt32;
                case CXTypeKind.CXType_ULongLong:
                    return BasicType.UInt64;
                case CXTypeKind.CXType_Char_S:
                case CXTypeKind.CXType_SChar:
                    return BasicType.Char;
                case CXTypeKind.CXType_WChar:
                case CXTypeKind.CXType_Short:
                    return BasicType.Int16;
                case CXTypeKind.CXType_Int:
                case CXTypeKind.CXType_Long:
                    return BasicType.Int32;
                case CXTypeKind.CXType_LongLong:
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
                case CXTypeKind.CXType_NullPtr:
                    return BasicType.NullPtr;
                case CXTypeKind.CXType_ConstantArray:
                case CXTypeKind.CXType_DependentSizedArray:
                case CXTypeKind.CXType_IncompleteArray:
                    return BasicType.Array;
                case CXTypeKind.CXType_LValueReference:
                    return BasicType.LValueReference;
                case CXTypeKind.CXType_RValueReference:
                    return BasicType.RValueReference;
                case CXTypeKind.CXType_Pointer:
                    return BasicType.Pointer;
                case CXTypeKind.CXType_Typedef:
                    return BasicType.Typedef;
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
            return IsUserDefinedTypeDecl(cursor.kind);
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

        public static bool IsVariadicTemplateParameter(CXCursor cursor)
        {
            List<string> tokens = ASTVisitor.GetCursorTokens(cursor);
            int count = tokens.Count;
            if (count < 2)
                return false;
            if (tokens[count - 1] == "...")
                return true;
            return tokens[count - 2] == "...";
        }

        public static string ToString(CXDiagnosticSeverity severity)
        {
            switch (severity)
            {
                case CXDiagnosticSeverity.CXDiagnostic_Error:
                    return "Error";
                case CXDiagnosticSeverity.CXDiagnostic_Fatal:
                    return "Fatal";
                case CXDiagnosticSeverity.CXDiagnostic_Ignored:
                    return "Ignore";
                case CXDiagnosticSeverity.CXDiagnostic_Note:
                    return "Note";
                case CXDiagnosticSeverity.CXDiagnostic_Warning:
                    return "Warning";
                default:
                    return "Unknown";
            }
        }

        public static bool IsFatal(CXDiagnosticSeverity severity)
        {
            return CXDiagnosticSeverity.CXDiagnostic_Fatal == severity ||
                CXDiagnosticSeverity.CXDiagnostic_Error == severity;
        }

        public static bool IsNonTypeTemplateParamLiteral(CXCursor cursor)
        {
            switch (cursor.kind)
            {
                case CXCursorKind.CXCursor_IntegerLiteral:
                case CXCursorKind.CXCursor_CXXBoolLiteralExpr:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsInvalid(CXCursor cursor)
        {
            return cursor.kind >= CXCursorKind.CXCursor_FirstInvalid &&
                cursor.kind <= CXCursorKind.CXCursor_LastInvalid;
        }

        public static bool IsTemplateRef(CXCursor cursor)
        {
            return CXCursorKind.CXCursor_TemplateRef == cursor.kind;
        }
    }
}
