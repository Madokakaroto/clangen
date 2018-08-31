﻿using System.Diagnostics;
using System.Collections.Generic;

namespace clangen
{
    public enum StructOrClass
    {
        Struct = -1,
        InDoubt = 0,
        Class = 1,
    }

    public enum AccessSpecifier
    {
        Invalid,
        Public,
        Protected,
        Private
    }

    public enum DefaultCompositeKind
    {
        None,
        Delete,
        Default
    }

    public class Field
    {
        public AccessSpecifier Access;
        public NativeType Type;
        public bool IsStatic;
    }

    public class BaseClass
    {
        public AccessSpecifier Access;
        public NativeClass Class;
        public bool IsVirtual;
    }

    public class SubClass
    {
        public AccessSpecifier Access;
        public NativeClass Class;
    }

    public class MemberType
    {
        public AccessSpecifier Access;
        public NativeType Type;
    }

    public class FunctionParameter
    {
        public string Name { get; set; }
        public NativeType Type { get; set; }
        public string DefaultValue { get; set; }
    }

    public class Method
    {
        private NativeClass class_;
        public string Name { get; }
        public bool IsStatic { get; }
        public bool IsConst { get; }
        public bool IsVirtual { get; }
        public bool IsAbstract { get; }

        public bool IsOverload { get; set; }
        public bool IsOverride { get; set; }

        public List<FunctionParameter> ParamList { get; }
            = new List<FunctionParameter>();
        public NativeType ResultType { get; set; }

        public Method(
            NativeClass @class,
            string name,
            bool isStatic,
            bool isConst,
            bool isVirtual,
            bool isAbstract)
        {
            Debug.Assert(!(isStatic && (isConst || isVirtual || isAbstract)));
            class_ = @class;
            Name = name;
            IsStatic = isStatic;
            IsConst = isConst;
            IsVirtual = isVirtual;
            IsAbstract = isAbstract;
        }

        public void AddParameter(FunctionParameter param)
        {
            ParamList.Add(param);
        }
    }

    public class Constructor
    {
        public bool IsConvert { get; }
        public bool IsCopy { get; }
        public bool IsMove { get; }
        public bool IsDefault { get; }
        public bool IsExplicit { get; set; } = false;
        public DefaultCompositeKind Composite { get; set; } = DefaultCompositeKind.None;
        public List<FunctionParameter> ParamList { get; }
            = new List<FunctionParameter>();

        public Constructor(
            bool isConvert, 
            bool isCopy, 
            bool isMove, 
            bool isDefault)
        {
            IsConvert = isConvert;
            IsCopy = isCopy;
            IsMove = isMove;
            IsDefault = isDefault;
        }

        public void AddParameter(FunctionParameter param)
        {
            ParamList.Add(param);
        }
    }
}