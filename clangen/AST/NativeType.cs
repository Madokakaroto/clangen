using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace clangen
{
    public enum BasicType
    {
        Unknown,
        Void,
        Bool,
        Char,
        Int8,
        Int16,
        Int32,
        Int64,
        UChar,
        UInt8,
        UInt16,
        UInt32,
        UInt64,
        Float,
        Double,
        LongDouble,
        Enum,
        Object
    }

    public enum QulifierType
    {
        LRef,
        RRef,
        Ptr,
        ConstPtr,
    }

    public class TypeQualifiers
    {
        private Stack<QulifierType> qulifiers;

        public TypeQualifiers()
        {
            qulifiers = new Stack<QulifierType>();
        }

        public void PushLValueReference()
        {
            qulifiers.Push(QulifierType.LRef);
        }

        public void PushRValueReference()
        {
            qulifiers.Push(QulifierType.RRef);
        }

        public void PushPointer()
        {
            qulifiers.Push(QulifierType.Ptr);
        }

        public void PushConstPointer()
        {
            qulifiers.Push(QulifierType.ConstPtr);
        }

        public Stack<QulifierType> GetStack()
        {
            return qulifiers;
        }
    }

    public class TypeInfo
    {
        public BasicType Type { get; private set; }
        private object Info;

        public TypeInfo()
        {
            Type = BasicType.Unknown;
        }

        public void SetBasicType(BasicType type)
        {
            Debug.Assert(ASTTraits.IsBuiltInType(type));
            Type = type;
            Info = null;
        }

        public void SetClass(NativeClass @class)
        {
            Type = BasicType.Object;
            Info = @class;
        }

        public void SetEnum(Enumeration @enum)
        {
            Type = BasicType.Enum;
            Info = @enum;
        }


    }

    public class NativeType
    {
        // property
        public TypeInfo Info { get; private set; }
        public TypeQualifiers Qualifiers { get; } = new TypeQualifiers();
        public string TypeName { get; }
        public bool IsConst { get; set; } = false;
        public bool IsTypedefed { get; private set; } = false;
        public NativeType TypedefedType { get; private set; } = null;
        private string collaspedName_ = "";

        public NativeType(string typeName)
        {
            TypeName = typeName;
            Info = new TypeInfo();
        }

        public void SetTypedefedType(NativeType type)
        {
            Debug.Assert(type != null);
            IsTypedefed = true;
            TypedefedType = type;
        }

        public string GetCollaspedName()
        {
            if(collaspedName_.Length == 0)
            {
                collaspedName_ = GenerateColloaspedName();
            }

            return collaspedName_;
        }

        private string GenerateColloaspedName()
        {
            if (IsTypedefed)
            {
                Debug.Assert(TypedefedType != null);
                string collaspedName = TypedefedType.GetCollaspedName();

                if (IsConst)
                    collaspedName += " const";

                foreach (QulifierType qType in Qualifiers.GetStack())
                {
                    switch (qType)
                    {
                        case QulifierType.LRef:
                            collaspedName += "&";
                            break;
                        case QulifierType.RRef:
                            collaspedName += "&&";
                            break;
                        case QulifierType.Ptr:
                            collaspedName += "*";
                            break;
                        case QulifierType.ConstPtr:
                            collaspedName += "* const";
                            break;
                    }
                }

                return collaspedName;
            }

            return TypeName;
        }
    }
}
