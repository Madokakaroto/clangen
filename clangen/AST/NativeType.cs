using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace clangen
{
    public enum BasicType
    {
        Unknown,
        Int8,
        Int16,
        Int32,
        Int64,
        UInt8,
        UInt16,
        UInt32,
        UInt64,
        Char,
        WChar,
        Float,
        Double,
        LDouble,
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
    }


    public class NativeType
    {
        // property
        public BasicType Type { get; set; } = BasicType.Unknown;
        public NativeClass Class { get; set; } = null;
        public TypeQualifiers Qualifiers { get; } = new TypeQualifiers();
        public string TypeName { get; }
        public bool IsConst { get; set; } = false;

        public NativeType(string typeName)
        {
            TypeName = typeName;
        }
    }


}
