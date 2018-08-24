using System;
using System.Collections.Generic;
using System.Text;

namespace clangen
{
    public class ASTTraits
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
}
