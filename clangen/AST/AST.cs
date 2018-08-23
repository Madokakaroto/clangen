using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace clangen
{
    public class AST
    {
        private Dictionary<string, NativeClass> classes_ = new Dictionary<string, NativeClass>();
        private Dictionary<string, NativeClass> unsettledClasses_ = new Dictionary<string, NativeClass>();
        private Dictionary<string, NativeType> types_ = new Dictionary<string, NativeType>();
        private Dictionary<string, Enumeration> enums_ = new Dictionary<string, Enumeration>();
        private Dictionary<string, Enumeration> unsettledEnums_ = new Dictionary<string, Enumeration>();

        public NativeClass GetClass(string className, out bool unsettled)
        {
            if(classes_.ContainsKey(className))
            {
                unsettled = false;
                return classes_[className];
            }
            else
            {
                if(!unsettledClasses_.ContainsKey(className))
                {
                    unsettledClasses_.Add(className, new NativeClass(className));
                }

                unsettled = true;
                return unsettledClasses_[className];
            }
        }

        public NativeClass GetClass(string className)
        {
            bool dummy = false;
            return GetClass(className, out dummy);
        }

        public void AddClass(NativeClass @class)
        {
            classes_.Add(@class.Name, @class);
            if(unsettledClasses_.ContainsKey(@class.Name))
            {
                unsettledClasses_.Remove(@class.Name);
            }
        }

        public NativeType GetType(string typeName, out bool unsettled)
        {
            if(types_.ContainsKey(typeName))
            {
                unsettled = false;
                return types_[typeName];
            }
            else
            {
                NativeType type = new NativeType(typeName);
                types_.Add(typeName, type);
                unsettled = true;
                return type;
            }
        }

        public Enumeration GetEnum(string enumName, out bool unsettled)
        {
            if(enums_.ContainsKey(enumName))
            {
                unsettled = false;
                return enums_[enumName];
            }
            else
            {
                if(!unsettledEnums_.ContainsKey(enumName))
                {
                    unsettledEnums_.Add(enumName, new Enumeration(enumName));
                }

                unsettled = true;
                return unsettledEnums_[enumName];
            }
        }

        public Enumeration GetEnum(string enumName)
        {
            bool dummy = false;
            return GetEnum(enumName, out dummy);
        }

        public void AddEnum(Enumeration @enum)
        {
            enums_.Add(@enum.Name, @enum);
            if(unsettledEnums_.ContainsKey(@enum.Name))
            {
                unsettledEnums_.Remove(@enum.Name);
            }
        }
    }
}
