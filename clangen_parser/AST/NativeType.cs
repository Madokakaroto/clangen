using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace clangen
{
    public enum BasicType
    {
        Unknown,
        Void,
        Bool,
        Char,
        Int8,
        Int16,
        Int32,
        Int64,
        UChar,
        UInt8,
        UInt16,
        UInt32,
        UInt64,
        Float,
        Double,
        LongDouble,
        Enum,
        Object
    }

    public enum QulifierType
    {
        Unknown,
        LRef,
        RRef,
        Ptr
    }

    public class TypeInfo : DotLiquid.Drop
    {
        public BasicType Type { get; private set; }
        private object Info;

        public TypeInfo()
        {
            Type = BasicType.Unknown;
        }

        public void SetBasicType(BasicType type)
        {
            Debug.Assert(ASTTraits.IsBuiltInType(type));
            Type = type;
            Info = null;
        }

        public void SetClass(NativeClass @class)
        {
            Type = BasicType.Object;
            Info = @class;
        }

        public void SetEnum(Enumeration @enum)
        {
            Type = BasicType.Enum;
            Info = @enum;
        }
    }

    public class NativeType : DotLiquid.Drop
    {
        // property
        public string TypeName { get; }
        public bool Parsed { get; set; } = false;
        public bool IsConst { get; set; } = false;
        public QulifierType Qualifier { get; set; } = QulifierType.Unknown;

        // builtin type, class or enum 
        public TypeInfo Info { get; private set; }

        // array
        public bool IsArray { get; set; } = false;
        public int Count { get; set; } = -1;

        // typedef or type alias
        public NativeType ReferencedType { get; private set; } = null;
        public string CollaspedName { get{ return GenerateColloaspedName(); } }

        public NativeType(string typeName)
        {
            TypeName = typeName;
        }

        public void SetReferencedType(NativeType type)
        {
            Debug.Assert(type != null);
            ReferencedType = type;
        }

        public void SetBasicType(BasicType type)
        {
            Debug.Assert(ASTTraits.IsBuiltInType(type));
            GetInfo().SetBasicType(type);
        }

        public void SetClass(NativeClass @class)
        {
            GetInfo().SetClass(@class);
        }

        public void SetEnum(Enumeration @enum)
        {
            GetInfo().SetEnum(@enum);
        }

        private TypeInfo GetInfo()
        {
            if (Info == null)
                Info = new TypeInfo();
            return Info;
        }

        public string GetCollaspedName()
        {
            return GenerateColloaspedName();
        }

        private string GenerateColloaspedName()
        {
            if (ReferencedType != null)
            {
                Debug.Assert(ReferencedType != null);
                string collaspedName = ReferencedType.CollaspedName;

                if (IsConst)
                    collaspedName += " const";

                switch (Qualifier)
                {
                    case QulifierType.LRef:
                        collaspedName += "&";
                        break;
                    case QulifierType.RRef:
                        collaspedName += "&&";
                        break;
                    case QulifierType.Ptr:
                        collaspedName += "*";
                        break;
                }

                return collaspedName;
            }

            return TypeName;
        }
    }

    public class DependentType
    {

    }
}
