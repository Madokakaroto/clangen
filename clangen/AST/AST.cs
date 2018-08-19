using System;
using System.Collections.Generic;
using System.Text;

namespace clangen
{
    class AST
    {
        private Dictionary<string, Class> classes_;
        private Dictionary<string, Class> unsettledClasses_;

        public AST()
        {
            classes_ = new Dictionary<string, Class>();
            unsettledClasses_ = new Dictionary<string, Class>();
        }

        public Class GetClass(string className, out bool unsettled)
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
                    unsettledClasses_.Add(className, new Class(className));
                }

                unsettled = true;
                return unsettledClasses_[className];
            }
        }

        public void AddClass(Class @class)
        {
            classes_.Add(@class.Name, @class);
            if(unsettledClasses_.ContainsKey(@class.Name))
            {
                unsettledClasses_.Remove(@class.Name);
            }
        }
    }
}
