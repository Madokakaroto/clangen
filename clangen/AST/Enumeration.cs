using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace clangen
{
    //[StructLayout(LayoutKind.Explicit)]
    public struct EnumConstant
    {
        /*[FieldOffset(0)]*/ public string     Name;
        /*[FieldOffset(1)]*/ public BasicType  UnderlyingType;
        /*[FieldOffset(2)]*/ public sbyte      Int8_Value;
        /*[FieldOffset(2)]*/ public byte       UInt8_Value;
        /*[FieldOffset(2)]*/ public short      Int16_Value;
        /*[FieldOffset(2)]*/ public ushort     UInt16_Value;
        /*[FieldOffset(2)]*/ public int        Int32_Value;
        /*[FieldOffset(2)]*/ public uint       UInt32_Value;
        /*[FieldOffset(2)]*/ public long       Int64_Value;
        /*[FieldOffset(2)]*/ public ulong      UInt64_Value;

        public static EnumConstant Create(string name, BasicType type, long value)
        {
            Debug.Assert(ASTTraits.IsSigned(type));

            EnumConstant c =  new EnumConstant
            {
                Name = name,
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

        public static EnumConstant Create(string name, BasicType type, ulong value)
        {
            Debug.Assert(ASTTraits.IsUnSigned(type));

            EnumConstant c =  new EnumConstant
            {
                Name = name,
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
                Name = "",
                UnderlyingType = BasicType.Unknown
            };
        }
    }


    public class Enumeration
    {
        public string Name { get; }
        private Dictionary<string, EnumConstant> constants_ 
            = new Dictionary<string, EnumConstant>();
        public bool IsEnumClass { get; set; } = false;

        public Enumeration(string name)
        {
            Name = name;
        }

        public void AddConstant(EnumConstant constant)
        {
            constants_.Add(constant.Name, constant);
        }

        EnumConstant GetConstant(string name)
        {
            if(constants_.ContainsKey(name))
                return constants_[name];

            return EnumConstant.CreateInvalid();
        }
    }
}
