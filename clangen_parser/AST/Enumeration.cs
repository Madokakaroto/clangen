using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace clangen
{
    [StructLayout(LayoutKind.Explicit)]
    public class EnumConstant
    {
        [FieldOffset(0)] public BasicType  UnderlyingType;
        [FieldOffset(4)] public sbyte      Int8_Value;
        [FieldOffset(4)] public byte       UInt8_Value;
        [FieldOffset(4)] public short      Int16_Value;
        [FieldOffset(4)] public ushort     UInt16_Value;
        [FieldOffset(4)] public int        Int32_Value;
        [FieldOffset(4)] public uint       UInt32_Value;
        [FieldOffset(4)] public long       Int64_Value;
        [FieldOffset(4)] public ulong      UInt64_Value;

        public static EnumConstant Create(BasicType type, long value)
        {
            Debug.Assert(ASTTraits.IsSigned(type));

            EnumConstant c =  new EnumConstant
            {
                UnderlyingType = type
            };

            switch (type)
            {
                case BasicType.Char:
                case BasicType.Int8:
                    c.Int8_Value = (sbyte)value;
                    break;
                case BasicType.Int16:
                    c.Int16_Value = (short)value;
                    break;
                case BasicType.Int32:
                    c.Int32_Value = (int)value;
                    break;
                case BasicType.Int64:
                    c.Int64_Value = value;
                    break;
            }

            return c;
        }

        public static EnumConstant Create(BasicType type, ulong value)
        {
            Debug.Assert(ASTTraits.IsUnSigned(type));

            EnumConstant c =  new EnumConstant
            {
                UnderlyingType = type
            };

            switch (type)
            {
                case BasicType.Char:
                case BasicType.Int8:
                    c.UInt8_Value = (byte)value;
                    break;
                case BasicType.Int16:
                    c.UInt16_Value = (ushort)value;
                    break;
                case BasicType.Int32:
                    c.UInt32_Value = (uint)value;
                    break;
                case BasicType.Int64:
                    c.UInt64_Value = value;
                    break;
            }

            return c;
        }

        public static EnumConstant CreateInvalid()
        {
            return new EnumConstant
            {
                UnderlyingType = BasicType.Unknown
            };
        }
    }

    public class EnumField
    {
        public string Name { get; }
        public EnumConstant Constant { get; }

        EnumField()
        {
            Constant = EnumConstant.CreateInvalid();
        }

        public EnumField(string name, BasicType type, long v)
        {
            Name = name;
            Constant = EnumConstant.Create(type, v);
        }

        public EnumField(string name, BasicType type, ulong v)
        {
            Name = name;
            Constant = EnumConstant.Create(type, v);
        }

        public static EnumField Invalid = new EnumField();
    }

    public class Enumeration
    {
        public string Name { get; }
        public bool Parsed { get; set; } = false;
        private Dictionary<string, EnumField> constants_ 
            = new Dictionary<string, EnumField>();
        public bool IsEnumClass { get; set; } = false;
        
        // for embedded 
        public bool IsEmbedded { get; set; } = false;
        public AccessSpecifier Access { get; set; } = AccessSpecifier.Private;
        public NativeClass OwnerClass { get; set; } = null;

        public Enumeration(string name)
        {
            Name = name;
        }

        public void AddConstant(EnumField constant)
        {
            constants_.Add(constant.Name, constant);
        }

        EnumField GetConstant(string name)
        {
            if(constants_.ContainsKey(name))
                return constants_[name];

            return EnumField.Invalid;
        }
    }
}
