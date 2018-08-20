using System;
using System.Collections.Generic;
using System.Text;

namespace clangen
{
    class MemberFunction
    {
        public string Name { get; }
        public bool IsStatic { get; } = false;
        public bool IsOverload { get; } = false;

        private bool isVirtual_ = false;
        private bool isConst_ = false;
        private bool isAbstract_ = false;
        private bool isOverride_ = false;

        public bool IsVirtual { get; }
        public bool IsOverride { get; }

        public MemberFunction(string name, bool isStatic)
        {
            Name = name;
            IsStatic = isStatic;
        }

        public bool IsConst
        {
            set
            {
                bool result = value;

                if (!IsStatic && result)
                {
                    isConst_ = result;
                }
                else
                    throw new Exception("Static function cannot be const!");
            }
            get
            {
                return isConst_;
            }
        }

        public bool IsAbstract
        {
            set
            {
                bool result = value;
                if (!IsStatic && result)
                {
                    isAbstract_ = result;
                }
                else
                    throw new Exception("Static function cannot be const!");
            }
            get
            {
                return isAbstract_;
            }
        }

        void SetVirtualOverride(bool isVirtual, bool isOverride)
        {
            if(isVirtual || isOverride && !IsStatic)
            {
                throw new Exception("Static function cannot be virtual or override!");
            }

            isVirtual_ = isVirtual;
            isOverride_ = isOverride;
        }
    }
}
