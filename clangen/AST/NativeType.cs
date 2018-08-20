using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace clangen
{
    enum BasicType
    {
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
        Object
    }

    enum QualifierType
    {
        Const,
        Ptr,
        LRef,
        RRef
    }

    class TypeQualifiers
    {
        private QualifierType[] qualifiers_;
        private uint qualifierCount;

        public TypeQualifiers()
        {
            qualifiers_ = new QualifierType[4];
            qualifierCount = 0;
        }

        public void AddQualifier(QualifierType @type)
        {
            qualifiers_[qualifierCount] = @type;
            qualifierCount++;
        }

        public QualifierType GetQualifier(uint index)
        {
            return qualifiers_[index];
        }
    }


    class NativeType
    {
        // property
        public BasicType Type { get; }
        public NativeClass Class { get; }
        public TypeQualifiers Qualifiers { get; } = new TypeQualifiers();

        NativeType(BasicType t)
        {
            Type = t;
            Class = null;
        }

        NativeType(NativeClass c)
        {
            Type = BasicType.Object;
            Class = c;
        }

        static NativeType CreateNativeType(BasicType type)
        {
            Debug.Assert(type != BasicType.Object);
            return new NativeType(type);
        }

        static NativeType CreateNativeType(NativeClass c)
        {
            return new NativeType(c);
        }

        public NativeType AddConst()
        {
            return AddQualifier(QualifierType.Const);
        }

        public NativeType AddPointer()
        {
            return AddQualifier(QualifierType.Ptr);
        }

        public NativeType AddLRef()
        {
            return AddQualifier(QualifierType.LRef);
        }

        public NativeType AddRRef()
        {
            return AddQualifier(QualifierType.RRef);
        }

        NativeType AddQualifier(QualifierType qualifierType)
        {
            Qualifiers.AddQualifier(qualifierType);
            return this;
        }
    }


}
