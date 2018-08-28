using System.Collections.Generic;

namespace clangen
{
    public class AST
    {
        private Dictionary<string, NativeClass> classes_ = new Dictionary<string, NativeClass>();
        private Dictionary<string, NativeClass> unsettledClasses_ = new Dictionary<string, NativeClass>();
        private Dictionary<string, NativeType> types_ = new Dictionary<string, NativeType>();
        private Dictionary<string, Enumeration> enums_ = new Dictionary<string, Enumeration>();
        private Dictionary<string, Enumeration> unsettledEnums_ = new Dictionary<string, Enumeration>();
        private Dictionary<string, List<NativeFunction>> functions_ = new Dictionary<string, List<NativeFunction>>();

        /* user defined type - class or struct */
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
            return GetClass(className, out bool dummy);
        }

        public void AddClass(NativeClass @class)
        {
            classes_.Add(@class.Name, @class);
            if(unsettledClasses_.ContainsKey(@class.Name))
            {
                unsettledClasses_.Remove(@class.Name);
            }
        }

        /* type */
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

        /* enum */
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
            return GetEnum(enumName, out bool dummy);
        }

        public void AddEnum(Enumeration @enum)
        {
            enums_.Add(@enum.Name, @enum);
            if(unsettledEnums_.ContainsKey(@enum.Name))
            {
                unsettledEnums_.Remove(@enum.Name);
            }
        }

        /* function */
        public NativeFunction GetFunction(string name, string type, out bool unsettled)
        {
            if(functions_.ContainsKey(name))
            {
                List<NativeFunction> functions = functions_[name];
                foreach(NativeFunction func in functions)
                {
                    if(func.TypeString == type)
                    {
                        unsettled = false;
                        return func;
                    }
                }

                NativeFunction newFunc = new NativeFunction(name, type, true);
                if (functions.Count == 1)
                {
                    functions[0].IsOverload = true;
                }
                functions.Add(newFunc);
                unsettled = true;
                return newFunc;
            }
            else
            {
                List<NativeFunction> functions = new List<NativeFunction>();
                functions_.Add(name, functions);
                NativeFunction newFunc = new NativeFunction(name, type);
                functions.Add(newFunc);
                unsettled = true;
                return newFunc;
            }
        }
    }
}
