using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace clangen
{
    public class EnumField : DotLiquid.Drop
    {
        public string Name { get; set; }
        public long Constant { get; set; }

        public static EnumField Invalid = new EnumField();
    }

    public class Enumeration : DotLiquid.Drop
    {
        public string Name { get; }
        public string UnscopedName { get; set; }
        public BasicType Type { get; set; }
        public bool Parsed { get; set; } = false;
        public Dictionary<string, EnumField> Fields { get; }
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
            Fields.Add(constant.Name, constant);
        }

        EnumField GetConstant(string name)
        {
            if (Fields.ContainsKey(name))
                return Fields[name];

            return EnumField.Invalid;
        }
    }
}
