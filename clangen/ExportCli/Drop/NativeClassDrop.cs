using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotLiquid;

namespace clangen.cli
{
    class NativeClassDrop : Drop
    {
        private static readonly Template h_template_;
        private static readonly Template cpp_template_;
        private readonly NativeClass class_;
        private object Extra { get; }

        static NativeClassDrop()
        {
            h_template_ = TemplateHelper.ParseTemplate("ExportCli/Templates/native_class_h.tmpl");
            cpp_template_ = TemplateHelper.ParseTemplate("ExportCli/Templates/native_class_cpp.tmpl");
        }

        public NativeClassDrop(NativeClass @class, object extra)
        {
            class_ = @class;
            Extra = extra;
        }

        public List<EnumerationDrop> SubEnums
        {
            get
            {
                List<EnumerationDrop> enums = new List<EnumerationDrop>();
                foreach(SubEnum subEnum in class_.SubEnums)
                {
                    var extra = new { Access = subEnum.Access.ToString() };
                    enums.Add(new EnumerationDrop(subEnum.Enum, extra));
                }
                return enums;
            }
        }
    }
}
