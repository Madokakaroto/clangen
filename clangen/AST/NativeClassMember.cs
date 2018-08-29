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
}
