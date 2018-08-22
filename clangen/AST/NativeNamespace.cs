using System;
using System.Collections.Generic;
using System.Text;

namespace clangen
{
    class NativeNamespace
    {
        private NativeNamespace parentNamespace_;
        private string namespaceName_;

        public NativeNamespace(string name)
        {
            namespaceName_ = name;
        }

        public NativeNamespace AddParent(string name)
        {
            parentNamespace_ = new NativeNamespace(name);
            return parentNamespace_;
        }

        string GetName(uint depth)
        {
            if (depth == 0)
                return namespaceName_;
            return namespaceName_ + "::";
        }

        string GetFullName(uint depth = 0)
        {
            if (parentNamespace_ == null)
                return GetName(depth);

            return parentNamespace_.GetFullName(depth + 1) + GetName(depth);
        }

        public string Name
        {
            get
            {
                return GetFullName();
            }
        }
    }
}
