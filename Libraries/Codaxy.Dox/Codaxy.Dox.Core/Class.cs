using System;
using System.Collections.Generic;

namespace Codaxy.Dox
{
    /// <summary>
    /// Defines elementary types (second tree level)
    /// </summary>
    public enum TypeKind { NONE, CLASS, INTERFACE, STRUCT, ENUM, DELEGATE }

    /// <summary>
    /// Defines level of protection
    /// </summary>
    public enum ProtectionLevel { PUBLIC, PRIVATE, PROTECTED, INTERNAL }

    [Flags]
    public enum MemberFlag
    {
        NONE = 0,
        CONSTRUCTOR = 1,
        DECONSTRUCTOR = 2,
        STATIC = 4,
        VIRTUAL = 8,
        ABSTRACT = 16,
        OVERRIDEN = 32,
        CONST = 64,
        EXPLICIT = 128,
        INLINE = 256,
    }

    /// <summary>
    /// Base class for any element
    /// </summary>
    public class CodeElement
    {
        public String Description { get; set; }

        public List<DescriptionSegment> DescriptionTree { get; set; }

        public bool HasDescription { get { return Description != null || DescriptionTree != null; } }

        public String Name { get; set; }

        public String RefId { get; set; }

        public override string ToString()
        {
            return Name ?? RefId;
        }
    }

    /// <summary>
    /// Type element which need to be any type of TypeKind (second tree level)
    /// </summary>
    public class Type : CodeElement
    {
        public List<string> BaseTypes { get; set; }

        public void AddBaseType(String baseType)
        {
            if (BaseTypes == null)
                BaseTypes = new List<string>();
            BaseTypes.Add(baseType);
        }

        public String Namespace { get; set; }

        public ProtectionLevel ProtectionLevel { get; set; }

        public TypeKind TypeKind { get; set; }

        public List<Method> Methods { get; set; }

        public List<Parameter> Parameters { get; set; }

        public List<Property> Properties { get; set; }

        public List<Field> Fields { get; set; }

        public List<Event> Events { get; set; }

        public List<MemberDelegate> MemberDelegates { get; set; }

        public string FullName { get { return Namespace != null ? Namespace + "." + Name : Name; } }
    }

    /// <summary>
    /// TypeCodeElement element which can be hold of a Type (thred tree level)
    /// </summary>
    public class TypedCodeElement : CodeElement
    {
        public String TypeName { get; set; }
    }

    /// <summary>
    /// Base class for property, method and field
    /// </summary>
    public class Member : TypedCodeElement
    {
        public MemberFlag MemberFlag { get; set; }

        public ProtectionLevel ProtectionLevel { get; set; }

        public string DeclaringType { get; set; }
    }

    /// <summary>
    /// Method
    /// </summary>
    public class Method : Member
    {
        public List<Parameter> Parameters { get; set; }

        public string ReturnDescription { get; set; }
    }

    [Flags]
    public enum ParameterFlags
    {
        None = 0,
        Ref = 1,
        Out = 2,
        Const = 4,
        This = 8
    }

    /// <summary>
    /// Parameter
    /// </summary>
    public class Parameter : TypedCodeElement
    {
        public ParameterFlags ParameterFlags { get; set; }
    }

    /// <summary>
    /// Property
    /// </summary>
    public class Property : Method
    {
        public bool CanRead { get; set; }

        public bool CanWrite { get; set; }
    }

    /// <summary>
    /// Field
    /// </summary>
    public class Field : Member
    {
        public String InitLiteral { get; set; }
    }

    public class Event : Member
    {
    }

    public class MemberDelegate : Method
    {
    }
}