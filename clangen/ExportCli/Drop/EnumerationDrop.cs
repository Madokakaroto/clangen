using System.Collections.Generic;
using DotLiquid;

namespace clangen.cli
{
    class EnumerationDrop : Drop
    {
        static private readonly Template template_;
        private readonly Enumeration enum_;
        public object Extra { get; }

        static EnumerationDrop()
        {
            template_ = TemplateHelper.ParseTemplate("ExportCli/Templates/enum_item.tmpl");
        }

        public EnumerationDrop(Enumeration @enum, object extra)
        {
            enum_ = @enum;
            Extra = extra;
        }

        public string Name
        {
            get { return enum_.UnscopedName; }
        }

        public bool IsDefault
        {
            get { return ASTTraits.IsDefaultEnumUnderlyingType(enum_); }
        }

        public string UnderlyingType
        {
            get { return enum_.Type.ToString(); }
        }

        public List<object> Fields
        {
            get
            {
                List<object> fields = new List<object>();
                foreach(EnumField field in enum_.Fields.Values)
                {
                    fields.Add(new
                    {
                        field.Name,
                        field.Constant
                    });
                }
                return fields;
            }
        }

        public string Render
        {
            get { return RenderResult(); }
        }

        private string RenderResult()
        {
            return template_.Render(DotLiquid.Hash.FromAnonymousObject(new
            {
                Enum = this
            }));
        }
    }
}
