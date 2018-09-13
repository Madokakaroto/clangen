using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace clangen
{
    public enum BasicType
    {
        Unknown,
        Void,
        NullPtr,
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
        Function,
        Enum,
        Object,
        // extend types
        Pointer,
        LValueReference,
        RValueReference,
        MemberFunctionPointer,
        MemberDataPointer,
        Array,
        Typedef
    }

    public class NativeArrayType
    {
        public int Count { get; set; }
        public NativeType Type { get; set; }
    }

    public class MemberFunctionPointer
    {
        public NativeClass Class { get; set; }
        public FunctionProto Function { get; set; }
    }

    public class MemberDataPointer
    {
        public NativeClass Class { get; set; }
        public NativeType Data { get; set; }
    }

    public class NativeType
    {
        // basic property
        public string TypeName { get; }
        public string UnscopedName { get; set; }
        public bool Parsed { get; set; } = false;
        public bool IsConst { get; set; } = false;

        // type info
        public BasicType TypeKind { get; set; } = BasicType.Unknown;
        public object Type { get; set; }

        // name
        public string CollapsedName { get { return GetCollapsedName(); } }

        public NativeType(string name)
        {
            TypeName = name;
        }

        void SetType(BasicType kind, object type)
        {
            TypeKind = kind;
            Type = type;
        }

        public void SetBuiltin(BasicType type)
        {
            Debug.Assert(ASTTraits.IsBuiltInType(type));
            SetType(type, null);
        }

        public void SetEnum(Enumeration e)
        {
            SetType(BasicType.Enum, e);
        }

        public void SetClass(NativeClass c)
        {
            SetType(BasicType.Object, c);
        }

        public void SetFunction(FunctionProto f)
        {
            SetType(BasicType.Function, f);
        }

        public void SetTypeLValRef(NativeType type)
        {
            SetType(BasicType.LValueReference, type);
        }

        public void SetTypeRValRef(NativeType type)
        {
            SetType(BasicType.RValueReference, type);
        }

        public void SetPointer(NativeType type)
        {
            SetType(BasicType.Pointer, type);
        }

        public void SetTypedef(NativeType type)
        {
            SetType(BasicType.Typedef, type);
        }

        public void SetArray(NativeArrayType arr)
        {
            SetType(BasicType.Array, arr);
        }

        public void SetPMF(MemberFunctionPointer pmf)
        {
            SetType(BasicType.MemberFunctionPointer, pmf);
        }

        public void SetPMD(MemberDataPointer pmd)
        {
            SetType(BasicType.MemberDataPointer, pmd);
        }

        public string GetCollapsedName(bool collapseTypedef = true, bool withConst = true)
        {
            bool valid = true;
            string result;
            if (ASTTraits.IsBuiltInType(TypeKind))
                result = ASTTraits.ToString(TypeKind);
            else if (ASTTraits.IsObject(TypeKind))
                result = (Type as NativeClass).Name;
            else if (ASTTraits.IsEnum(TypeKind))
                result = (Type as Enumeration).Name;
            else if (ASTTraits.IsPointer(TypeKind))
                result = string.Format("{0} *", (Type as NativeType).CollapsedName);
            else if (ASTTraits.IsLValueReference(TypeKind))
                result = string.Format("{0} &", (Type as NativeType).CollapsedName);
            else if (ASTTraits.IsRValueReference(TypeKind))
                result = string.Format("{0} &&", (Type as NativeType).CollapsedName);
            else if (ASTTraits.IsTypedef(TypeKind))
            {
                Debug.Assert(!IsConst);
                if (collapseTypedef)
                    result = (Type as NativeType).CollapsedName;
                else
                {
                    if (!withConst)
                        result = StringTools.RemoveFirstOf(TypeName, "const");
                    else 
                        result = TypeName;
                }
            }
            else
            {
                valid = false;
                result = "unknown";
            }

            if (valid && IsConst && withConst)
                result += " const";

            return result;
        }
    }

    public class DependentType
    {

    }
}
