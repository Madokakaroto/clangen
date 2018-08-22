using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace clangen
{
    class AST
    {
        private Dictionary<string, NativeClass> classes_;
        private Dictionary<string, NativeClass> unsettledClasses_;

        public AST()
        {
            classes_ = new Dictionary<string, NativeClass>();
            unsettledClasses_ = new Dictionary<string, NativeClass>();
        }

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
    }
}
