using System;
using System.Collections.Generic;
using System.Text;

namespace clangen
{
    // basic traits
    public partial class ASTTraits
    {
        public static bool IsIntegral(BasicType type)
        {
            return type >= BasicType.Bool &&
                type <= BasicType.UChar;
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
            return type != BasicType.Object &&
                type != BasicType.Enum &&
                type != BasicType.Unknown;
        }
    }

    // for enumeration
    public partial class ASTTraits
    {
        public static bool IsEnum(BasicType type)
        {
            return type == BasicType.Enum;
        }

        public static bool IsDefaultEnumUnderlyingType(Enumeration @enum)
        {
            return @enum.Type == BasicType.Int32;
        }
    }

    // for template
    public partial class ASTTraits
    {
        public static bool IsTemplateParamType(NativeType type)
        {
            return false;
        }
    }

}
