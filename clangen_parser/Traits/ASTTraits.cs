using System.Diagnostics;

namespace clangen
{
    // basic traits
    public partial class ASTTraits
    {
        public static bool IsIntegral(BasicType type)
        {
            return type >= BasicType.Bool &&
                type <= BasicType.UInt64;
        }

        public static bool IsFloatingPoint(BasicType type)
        {
            return BasicType.Float == type ||
                BasicType.Double == type ||
                BasicType.LongDouble == type;
        }

        public static bool IsArithmatic(BasicType type)
        {
            return IsIntegral(type) || IsFloatingPoint(type);
        }

        public static bool IsSigned(BasicType type)
        {
            return type >= BasicType.Char &&
               type <= BasicType.Int64;
        }

        public static bool IsUnSigned(BasicType type)
        {
            return type >= BasicType.UChar &&
               type <= BasicType.UInt64;
        }

        public static bool IsBuiltInType(BasicType type)
        {
            return type > BasicType.Unknown &&
                type < BasicType.Function;
        }

        public static bool IsTypedef(BasicType type)
        {
            return BasicType.Typedef == type;
        }

        public static bool IsEnum(BasicType type)
        {
            return BasicType.Enum == type;
        }

        public static bool IsNullPtr(BasicType type)
        {
            return BasicType.NullPtr == type;
        }

        public static bool IsDefaultEnumUnderlyingType(Enumeration e)
        {
            return e.Type == BasicType.Int32;
        }

        public static bool IsObject(BasicType type)
        {
            return BasicType.Object == type;
        }

        public static bool IsPointer(BasicType type)
        {
            return BasicType.Pointer == type;
        }

        public static bool IsLValueReference(BasicType type)
        {
            return BasicType.LValueReference == type;
        }

        public static bool IsRValueReference(BasicType type)
        {
            return BasicType.RValueReference == type;
        }

        public static bool IsReference(BasicType type)
        {
            return IsLValueReference(type) || IsRValueReference(type);
        }

        public static bool IsMemberFunctionPointer(BasicType type)
        {
            return BasicType.MemberFunctionPointer == type;
        }

        public static bool IsMemberDataPointer(BasicType type)
        {
            return BasicType.MemberDataPointer == type;
        }

        public static bool IsMemberPointer(BasicType type)
        {
            return IsMemberFunctionPointer(type) || IsMemberDataPointer(type);
        }
    }

    // for class type
    public partial class ASTTraits
    {
        public static bool IsInstanceOf(NativeClass @class, string templateName)
        {
            if (!@class.IsTemplateInstantiation || @class.InstanceOf == null)
                return false;

            return @class.InstanceOf.Spelling == templateName;
        }
    }

    // for native type
    public partial class ASTTraits
    {
        public static bool IsTemplateParamType(NativeType type)
        {
            return false;
        }

        public static bool IsBuiltInType(NativeType type)
        {
            if (IsBuiltInType(type.TypeKind))
                return true;
            else if (IsTypedef(type.TypeKind))
                return IsBuiltInType(type.Type as NativeType);
            else
                return false;
        }

        public static bool IsIntegral(NativeType type)
        {
            if (IsIntegral(type.TypeKind))
                return true;
            else if (IsTypedef(type.TypeKind))
                return IsIntegral(type.Type as NativeType);
            else
                return false;
        }

        public static bool IsFloatingPoint(NativeType type)
        {
            if (IsFloatingPoint(type.TypeKind))
                return true;
            else if (IsTypedef(type.TypeKind))
                return IsFloatingPoint(type.Type as NativeType);
            else
                return false;
        }

        public static bool IsArithmatic(NativeType type)
        {
            if (IsArithmatic(type.TypeKind))
                return true;
            else if (IsTypedef(type.TypeKind))
                return IsArithmatic(type.Type as NativeType);
            else
                return false;
        }

        public static bool IsSigned(NativeType type)
        {
            if (IsSigned(type.TypeKind))
                return true;
            else if (IsTypedef(type.TypeKind))
                return IsSigned(type.Type as NativeType);
            else
                return false;
        }

        public static bool IsUnSigned(NativeType type)
        {
            if (IsUnSigned(type.TypeKind))
                return true;
            else if (IsTypedef(type.TypeKind))
                return IsUnSigned(type.Type as NativeType);
            else
                return false;
        }

        public static bool IsEnum(NativeType type)
        {
            if (IsEnum(type.TypeKind))
                return true;
            else if (IsTypedef(type.TypeKind))
                return IsEnum(type.Type as NativeType);
            else
                return false;
        }

        public static bool IsObject(NativeType type)
        {
            if (IsObject(type.TypeKind))
                return true;
            else if (IsTypedef(type.TypeKind))
                return IsObject(type.Type as NativeType);
            else
                return false;
        }

        public static bool IsReference(NativeType type)
        {
            if (IsReference(type.TypeKind))
                return true;
            else if (IsTypedef(type.TypeKind))
                return IsReference(type.Type as NativeType);
            else
                return false;
        }

        public static bool IsLValueReference(NativeType type)
        {
            if (IsLValueReference(type.TypeKind))
                return true;
            else if (IsTypedef(type.TypeKind))
                return IsLValueReference(type.Type as NativeType);
            else
                return false;
        }

        public static bool IsRValueReference(NativeType type)
        {
            if (IsRValueReference(type.TypeKind))
                return true;
            else if (IsTypedef(type.TypeKind))
                return IsRValueReference(type.Type as NativeType);
            else
                return false;
        }

        public static bool IsPointer(NativeType type)
        {
            if (IsPointer(type.TypeKind))
                return true;
            else if (IsTypedef(type.TypeKind))
                return IsPointer(type.Type as NativeType);
            else
                return false;
        }

        public static bool IsTypedef(NativeType type)
        {
            return IsTypedef(type.TypeKind);
        }

        public static bool IsInstanceOf(NativeType type, string name)
        {
            if (IsObject(type.TypeKind))
                return IsInstanceOf(type.Type as NativeClass, name);
            else if (IsTypedef(type.TypeKind))
                return IsInstanceOf(type.Type as NativeType, name);
            else
                return false;
        }

        public static bool IsTemplateNoTypeParameterType(NativeType type)
        {
            if (IsIntegral(type.TypeKind) ||
                IsPointer(type.TypeKind) ||
                IsMemberPointer(type.TypeKind) ||
                IsEnum(type.TypeKind) ||
                IsNullPtr(type.TypeKind))
                return true;
            else if (IsTypedef(type.TypeKind))
                return IsTemplateNoTypeParameterType(type.Type as NativeType);
            else
                return false;
        }

        public static string GetClassName(NativeType type)
        {
            if (IsObject(type.TypeKind))
                return (type.Type as NativeClass).Name;
            else if (IsTypedef(type.TypeKind))
                return GetClassName(type.Type as NativeType);
            else
                return null;
        }

        public static NativeType RemovePointer(NativeType type)
        {
            if (IsPointer(type.TypeKind))
                return type.Type as NativeType;
            else if (IsTypedef(type.TypeKind))
                return RemovePointer(type.Type as NativeType);
            else
                return type;
        }

        public static NativeType RemoveReference(NativeType type)
        {
            if (IsReference(type.TypeKind))
                return type.Type as NativeType;
            else if (IsTypedef(type.TypeKind))
                return RemoveReference(type.Type as NativeType);
            else
                return type;
        }

        public static NativeType KeyType(NativeType type)
        {
            return null;
        }

        public static NativeType MappedType(NativeType type)
        {
            return null;
        }

        public static bool IsSameType(NativeType lhs, NativeType rhs)
        {
            return true;
        }
    }

    // misc
    public partial class ASTTraits
    {
        public static string ToString(AccessSpecifier access)
        {
            switch (access)
            {
                case AccessSpecifier.Public:
                    return "public";
                case AccessSpecifier.Protected:
                    return "protected";
                case AccessSpecifier.Private:
                    return "private";
                default:
                    Debug.Assert(false);
                    return "";
            }
        }

        public static string ToString(BasicType type)
        {
            switch(type)
            {
                case BasicType.Void:
                    return "void";
                case BasicType.Bool:
                    return "bool";
                case BasicType.Char:
                    return "char";
                case BasicType.Int8:
                    return "int8_t";
                case BasicType.Int16:
                    return "int16_t";
                case BasicType.Int32:
                    return "int32_t";
                case BasicType.Int64:
                    return "int64_t";
                case BasicType.UInt8:
                    return "uint8_t";
                case BasicType.UInt16:
                    return "uint16_t";
                case BasicType.UInt32:
                    return "uint32_t";
                case BasicType.UInt64:
                    return "uint64_t";
                case BasicType.Float:
                    return "float";
                case BasicType.Double:
                    return "double";
                case BasicType.LongDouble:
                    return "long double";
                default:
                    Debug.Assert(false);
                    return "";
            }
        }
    }

}
