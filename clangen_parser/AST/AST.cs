using System.Collections.Generic;

namespace clangen
{
    public class AST : DotLiquid.Drop
    {
        private Dictionary<string, NativeClass> classes_ = new Dictionary<string, NativeClass>();
        private Dictionary<string, NativeType> types_ = new Dictionary<string, NativeType>();
        private Dictionary<string, Enumeration> enums_ = new Dictionary<string, Enumeration>();
        private Dictionary<string, List<NativeFunction>> functions_ = new Dictionary<string, List<NativeFunction>>();
        private Dictionary<string, ClassTemplate> templates_ = new Dictionary<string, ClassTemplate>();

        /* user defined type - class or struct */
        public NativeClass GetClass(string className)
        {
            if(classes_.ContainsKey(className))
            {
                return classes_[className];
            }
            else
            {
                NativeClass newClass = new NativeClass(className);
                classes_.Add(className, newClass);
                return newClass;
            }
        }

        /* type */
        public NativeType GetType(string typeName)
        {
            if(types_.ContainsKey(typeName))
            {
                return types_[typeName];
            }
            else
            {
                NativeType type = new NativeType(typeName);
                types_.Add(typeName, type);
                return type;
            }
        }

        /* enum */
        public Enumeration GetEnum(string enumName)
        {
            if(enums_.ContainsKey(enumName))
            {
                return enums_[enumName];
            }
            else
            {
                Enumeration @enum = new Enumeration(enumName);
                enums_.Add(enumName, @enum);
                return @enum;
            }
        }

        /* function */
        public NativeFunction GetFunction(string name, string type)
        {
            if(functions_.ContainsKey(name))
            {
                List<NativeFunction> functions = functions_[name];
                foreach(NativeFunction func in functions)
                {
                    if(func.TypeString == type)
                    {
                        return func;
                    }
                }

                NativeFunction newFunc = new NativeFunction(name, type, true);
                if (functions.Count == 1)
                {
                    functions[0].IsOverload = true;
                }
                functions.Add(newFunc);
                return newFunc;
            }
            else
            {
                List<NativeFunction> functions = new List<NativeFunction>();
                functions_.Add(name, functions);
                NativeFunction newFunc = new NativeFunction(name, type);
                functions.Add(newFunc);
                return newFunc;
            }
        }

        public ClassTemplate GetClassTemplate(string name)
        {
            if(templates_.ContainsKey(name))
            {
                return templates_[name];
            }
            else
            {
                ClassTemplate template = new ClassTemplate(name);
                templates_.Add(name, template);
                return template;
            }
        }
    }
}
