using System.Collections.Generic;

namespace clangen
{
    class EnumerationDrop : DotLiquid.Drop
    {
        public string Name { get; set; }
        public bool IsDefault { get; set; } = true;
        public string UnderlyingType { get; set; }
        public List<object> Fields { get; set; } = new List<object>();

        public EnumerationDrop(Enumeration @enum)
        {
            DropName(@enum);
            DropUnderlyingType(@enum);
            DropFields(@enum);
        }

        void DropName(Enumeration @enum)
        {
            Name = @enum.UnscopedName;
        }

        void DropFields(Enumeration @enum)
        {
            foreach(EnumField field in @enum.Fields.Values)
            {
                Fields.Add(new
                {
                    field.Name,
                    field.Constant
                });
            }
        }

        void DropUnderlyingType(Enumeration @enum)
        {
            IsDefault = ASTTraits.IsDefaultEnumUnderlyingType(@enum);
            UnderlyingType = @enum.Type.ToString();
        }
    }
}
