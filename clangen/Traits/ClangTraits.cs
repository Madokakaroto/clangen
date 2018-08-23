using System;
using System.Collections.Generic;
using System.Text;
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

        public static bool IsTerminalType(CXType type)
        {
            return IsBuiltInType(type) || IsEnum(type) || IsUserDefiendType(type);
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
                default:
                    return BasicType.Unknown;
            }
        }
    }
}
