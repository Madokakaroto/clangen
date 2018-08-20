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

        public NativeClass GetClass(string className, StructOrClass classTag, out bool unsettled)
        {
            if(classes_.ContainsKey(className))
            {
                unsettled = false;
                NativeClass findClass = classes_[className];
                if(findClass.ClassTag == StructOrClass.InDoubt)
                {
                    findClass.ClassTag = classTag;
                }

                if(findClass.ClassTag != StructOrClass.InDoubt && classTag != StructOrClass.InDoubt)
                {
                    Debug.Assert(findClass.ClassTag == classTag);
                }
                return findClass;
            }
            else
            {
                if(!unsettledClasses_.ContainsKey(className))
                {
                    unsettledClasses_.Add(className, new NativeClass(className, classTag));
                }

                unsettled = true;
                return unsettledClasses_[className];
            }
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
