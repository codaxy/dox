using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Codaxy.Common.Logging;

namespace Codaxy.Dox.Providers
{
    //    <members>
    //        <member name="T:DemoLib.DocumentedClass">
    //            <summary>
    //            Class summary
    //            </summary>
    //        </member>
    //        <member name="F:DemoLib.DocumentedClass.documentedConstField">
    //            <summary>
    //            const property
    //            </summary>
    //        </member>
    //        <member name="M:DemoLib.DocumentedClass.DocumentedMethod(System.Int32,System.Boolean)">
    //            <summary>
    //            Method Summary
    //            </summary>
    //            <param name="param1">Param1 summary</param>
    //            <param name="param2">Param2 summary</param>
    //            <returns>Return summary</returns>
    //            <abc>aaaaa</abc>
    //        </member>

    class XmlCommentsDoxProviderImpl : IDisposable
    {
        public XmlCommentsDoxProviderImpl(IDoxBuilder builder, IEnumerable<string> sourceFiles)
        {
            DoxBuilder = builder;
            SourceFiles = sourceFiles;
            typeCache = new Dictionary<string, Type>();
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += MyReflectionOnlyResolveEventHandler;
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= MyReflectionOnlyResolveEventHandler;
        }

        public Logger Logger { get; set; }

        IDoxBuilder DoxBuilder { get; set; }

        IEnumerable<string> SourceFiles { get; set; }

        Dictionary<String, Type> typeCache;

        public void Process()
        {
            foreach (var path in SourceFiles)
            {
                ProcessAssembly(path);
                ProcessAssemblyComments(path);
            }

            UpdateInheritedMemberDescriptions();

            foreach (var t in typeCache.Values)
            {
                var dox = new DoxDocument
                {
                    Type = t,
                    Title = t.Name,
                    FullName = t.FullName,
                    FormatCode = "cs"
                };
                dox.GenerateTypeSearchItems();
                DoxBuilder.Add(dox);
            }
        }

        private void UpdateInheritedMemberDescriptions()
        {
            foreach (var t in typeCache.Values)
            {
                Type dt;
                if (t.Methods != null)
                    for (var i = 0; i < t.Methods.Count; i++)
                    {
                        var m = t.Methods[i];
                        if (!m.HasDescription && m.DeclaringType != null && m.DeclaringType != t.FullName && typeCache.TryGetValue(m.DeclaringType, out dt) && dt.Methods != null)
                        {
                            var method = dt.Methods.Find(a => a.RefId == m.RefId);
                            if (method != null)
                                t.Methods[i] = method;
                        }
                    }

                if (t.Properties != null)
                    for (var i = 0; i < t.Properties.Count; i++)
                    {
                        var p = t.Properties[i];
                        if (!p.HasDescription && p.DeclaringType != null && p.DeclaringType != t.FullName && typeCache.TryGetValue(p.DeclaringType, out dt) && dt.Properties != null)
                        {
                            var property = dt.Properties.Find(a => a.RefId == p.RefId);
                            if (property != null)
                                t.Properties[i] = property;
                        }
                    }

                if (t.Fields != null)
                    for (var i = 0; i < t.Fields.Count; i++)
                    {
                        var f = t.Fields[i];
                        if (!f.HasDescription && f.DeclaringType != null && f.DeclaringType != t.FullName && typeCache.TryGetValue(f.DeclaringType, out dt) && dt.Fields != null)
                        {
                            var field = dt.Fields.Find(a => a.RefId == f.RefId);
                            if (field != null)
                                t.Fields[i] = field;
                        }
                    }
            }
        }

        private void ProcessAssemblyComments(string path)
        {
            path = Path.ChangeExtension(path, "xml");
            if (File.Exists(path))
                using (var fr = XmlReader.Create(path))
                {
                    XDocument xdoc = XDocument.Load(fr);
                    foreach (var member in xdoc.Descendants("member"))
                    {
                        var nameAtt = member.Attribute("name");
                        if (nameAtt != null && !String.IsNullOrEmpty(nameAtt.Value))
                        {
                            var memberTypeCode = nameAtt.Value[0];
                            var name = nameAtt.Value.Substring(2);
                            switch (memberTypeCode)
                            {
                                case 'T':
                                    GetType(name).ParseSummary(member);
                                    break;
                                case 'M':
                                    var m = GetMethod(name);
                                    m.ParseSummary(member);
                                    if (m.Parameters != null)
                                        foreach (var pdesc in member.Descendants("param"))
                                        {
                                            var pname = pdesc.Attribute("name").SafeGetValue();
                                            if (pname != null)
                                            {
                                                var p = m.Parameters.Find(a => a.Name == pname);
                                                if (p != null)
                                                    p.ParseDescription(pdesc);
                                            }
                                        }
                                    foreach (var rdesc in member.Descendants("returns"))
                                        m.ReturnDescription = rdesc.Value;
                                    break;
                                case 'F':
                                    GetField(name).ParseSummary(member);
                                    break;
                                case 'P':
                                    var prop = GetProperty(name);
                                    prop.ParseSummary(member);
                                    if (prop.Parameters != null)
                                        foreach (var pdesc in member.Descendants("param"))
                                        {
                                            var pname = pdesc.Attribute("name").SafeGetValue();
                                            if (pname != null)
                                            {
                                                var p = prop.Parameters.Find(a => a.Name == pname);
                                                if (p != null)
                                                    p.ParseDescription(pdesc);
                                            }
                                        }									
                                    break;
                                case 'E':
                                    var ev = GetEvent(name);
                                    ev.ParseSummary(member);
                                    break;
                            }
                        }
                    }
                }
        }

        String currentAssembly;

        private Assembly MyReflectionOnlyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            AssemblyName name = new AssemblyName(args.Name);
            String asmToCheck = Path.GetDirectoryName(currentAssembly) + "\\" + name.Name + ".dll";

            if (File.Exists(asmToCheck))
            {
                return Assembly.ReflectionOnlyLoadFrom(asmToCheck);
            }

            return Assembly.ReflectionOnlyLoad(args.Name);
        }

        private void ProcessAssembly(string path)
        {
            path = Path.ChangeExtension(path, "dll");
            if (File.Exists(path))
            {
                System.Type[] types;
                try
                {
                    var fileInfo = new FileInfo(path);
                    if (!fileInfo.Exists)
                        throw new InvalidOperationException(String.Format("File '{0}' does not exist.", path));
                    currentAssembly = fileInfo.FullName;
                    //var assembly = Assembly.LoadFile(fileInfo.FullName);
                    var assembly = Assembly.ReflectionOnlyLoadFrom(fileInfo.FullName);

                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    foreach (var lex in ex.LoaderExceptions)
                        Logger.Exception(lex);
                    throw;
                }

                foreach (var type in types)
                    if (!type.IsSpecialName && !type.Name.StartsWith("<") && (type.IsPublic || type.IsNestedPublic))
                    {
                        var fullName = type.FullName;
                        var mtype = GetType(fullName, true);
                        if (type.IsClass)
                            mtype.TypeKind = TypeKind.CLASS;
                        else if (type.IsEnum)
                            mtype.TypeKind = TypeKind.ENUM;
                        else if (type.IsInterface)
                            mtype.TypeKind = TypeKind.INTERFACE;

                        mtype.Name = GetTypeName(type, false);
                        if (type.BaseType != null)
                            mtype.AddBaseType(GetReadableTypeName(type.BaseType));
                        foreach (var interf in type.GetInterfaces())
                            mtype.AddBaseType(GetReadableTypeName(interf));

                        foreach (var property in type.GetProperties())
                        {
							if (property.DeclaringType.Namespace.StartsWith("System"))
								continue;
                            if (mtype.Properties == null)
                                mtype.Properties = new List<Property>();
                            var parameters = property.GetIndexParameters();
                            mtype.Properties.Add(new Property
                            {
                                Name = parameters != null && parameters.Length > 0 && property.Name == "Item" ? "this" : property.Name,
                                RefId = GetPropertyRefId(property, parameters),
                                Parameters = BuildParameterList(parameters, false),
                                TypeName = GetReadableTypeName(property.PropertyType),
                                CanRead = property.CanRead,
                                CanWrite = property.CanWrite,
                                ProtectionLevel = ExtractProtectionLevel(property),
                                DeclaringType = GetReadableTypeName(property.DeclaringType),
                                MemberFlag = GetMemberFlags(property.GetGetMethod() ?? property.GetSetMethod())
                            });
                        }

                        foreach (var field in type.GetFields())
                            if (!field.IsSpecialName)
                            {
                                if (mtype.Fields == null)
                                    mtype.Fields = new List<Field>();
                                mtype.Fields.Add(new Field
                                {
                                    Name = field.Name,
                                    RefId = field.Name,
                                    TypeName = GetReadableTypeName(field.FieldType),
                                    DeclaringType = GetReadableTypeName(field.DeclaringType),
                                    MemberFlag = GetMemberFlags(field),
                                    InitLiteral = GetFieldInitLiteral(field)
                                });
                            }

                        var methods = new List<MethodBase>();
                        foreach (var c in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic))
                            if (c.IsFamily || c.IsPublic)
                                methods.Add(c);
                        foreach (var m in type.GetMethods()) //BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic))
                            if (!m.IsConstructor && (m.IsFamily || m.IsPublic))
                                methods.Add(m);
                        foreach (var method in methods)
                            try
                            {
								if (method.DeclaringType.Namespace.StartsWith("System"))
									continue;
								if (method.IsConstructor || !(method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_"))))
                                {
                                    if (mtype.Methods == null)
                                        mtype.Methods = new List<Method>();
                                    var parameters = method.GetParameters();
                                    var name = method.Name;
									if (method.IsConstructor)
										name = GetConstructorName(type);

                                    var m = new Method
                                    {
                                        RefId = GetMethodRefId(method, parameters),
                                        Name = name,
                                        Parameters = BuildParameterList(parameters, type.IsSealed && !type.IsGenericType && method.IsStatic),
                                        ProtectionLevel = ExtractProtectionLevel(method),
                                        DeclaringType = GetReadableTypeName(method.DeclaringType),
                                        TypeName = method.IsConstructor ? null : GetReadableTypeName(((MethodInfo)method).ReturnType),
                                        MemberFlag = GetMemberFlags(method)
                                    };
                                    mtype.Methods.Add(m);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Exception("Exception for type: " + type + ", method: " + method.Name, ex);
                            }

                        var events = type.GetEvents();
                        foreach (var evnt in events)
                        {
                            try
                            {
                                if (mtype.Events == null)
                                    mtype.Events = new List<Event>();
                                var e = new Event
                                {
                                    Name = evnt.Name,
                                    RefId = evnt.Name,
                                    TypeName = GetReadableTypeName(evnt.EventHandlerType),
                                    DeclaringType = GetReadableTypeName(evnt.DeclaringType),
                                    MemberFlag = GetMemberFlags(evnt.GetAddMethod())
                                };
                                mtype.Events.Add(e);
                            }
                            catch (Exception ex)
                            {
                                Logger.Exception("Exception for type: " + type + ", event: " + evnt.Name, ex);
                            }
                        }
                    }
            }
        }

        private List<Parameter> BuildParameterList(ParameterInfo[] parameters, bool checkForExtensionMethods)
        {
            if (parameters == null || parameters.Length == 0)
                return null;
            return parameters.Select(a => new Parameter
            {
                Name = a.Name,
                TypeName = GetReadableTypeName(a.ParameterType),
                ParameterFlags = GetParameterFlags(a, checkForExtensionMethods)
            }).ToList();
        }

        private string GetMethodRefId(MethodBase method, ParameterInfo[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(method.Name.Replace(".ctor", "#ctor"));
            if (method.IsGenericMethod)
            {
                var gp = method.GetGenericArguments();
                if (gp.Length > 0)
                    sb.Append("``").Append(gp.Length);
            }
            sb.Append("(");
            sb.Append(string.Join(",", parameters.Select(a => a.ParameterType.IsGenericParameter ? (a.ParameterType.DeclaringMethod != null ? "``" : "`") + a.ParameterType.GenericParameterPosition : GetReadableTypeName(a.ParameterType)).ToArray()));
            sb.Append(")");

            return sb.ToString().Replace('<', '{').Replace('>', '}').Replace(" ", "");
        }

        private string GetPropertyRefId(PropertyInfo pinfo, ParameterInfo[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(pinfo.Name);
            if (parameters != null && parameters.Length > 0)
            {
                sb.Append("(");
                sb.Append(string.Join(",", parameters.Select(a => a.ParameterType.IsGenericParameter ? (a.ParameterType.DeclaringMethod != null ? "``" : "`") + a.ParameterType.GenericParameterPosition : GetReadableTypeName(a.ParameterType)).ToArray()));
                sb.Append(")");
            }

            return sb.ToString().Replace('<', '{').Replace('>', '}').Replace(" ", "");
        }

        private String BuildTemplateParamsString(System.Type[] genericArguments)
        {
            if (genericArguments.Length == 0)
                return null;
            var sb = new StringBuilder();
            sb.Append("<");
            bool first = true;
            foreach (var gt in genericArguments)
            {
                if (first) first = false; else sb.Append(", ");
                sb.Append(gt.IsGenericParameter ? gt.ToString() : GetReadableTypeName(gt));
            }
            sb.Append(">");
            return sb.ToString();
        }

        private ParameterFlags GetParameterFlags(ParameterInfo pinfo, bool checkForExtensionMethods)
        {
            ParameterFlags res = ParameterFlags.None;
            if (pinfo.IsOut)
                res |= ParameterFlags.Out;
            else if (pinfo.ParameterType.IsByRef)
                res |= ParameterFlags.Ref;
            if (pinfo.Position == 0 && checkForExtensionMethods)
                foreach (var cad in CustomAttributeData.GetCustomAttributes(pinfo))
                    if (cad.Constructor.DeclaringType == typeof(ExtensionAttribute))
                        res |= ParameterFlags.This;
            return res;
        }

        private string GetFieldInitLiteral(FieldInfo field)
        {
            if (!field.IsLiteral)
                return null;
            var value = field.GetRawConstantValue();
            if (field.FieldType == typeof(String))
                return "\"" + value.ToString() + "\"";
            return value.ToString();
        }

        //private string GetFullTypeName(System.Type type) { return GetTypeName(type, true, null); }

        //private string GetReadableTypeName(System.Type type, String ns)
        //{
        //    return TrimTypeName(GetTypeName(type, true, ns), ns, TrimNs);
        //}

        private string GetReadableTypeName(System.Type type)
        {
            return GetTypeName(type, true);
        }

        private string GetTypeName(System.Type type, bool full)
        {			
            if (type.IsGenericParameter)
                return type.ToString();
            if (type.IsByRef && type.HasElementType)
                return GetTypeName(type.GetElementType(), full);
			if (type.IsArray)
				return GetTypeName(type.GetElementType(), true) + "[]";

			String prefix = "";			
			if (type.IsNestedPublic)
				prefix = GetTypeName(type.DeclaringType, false) + "+" + prefix;

			if (type.IsGenericType || type.IsGenericTypeDefinition)
			{
				StringBuilder sb = new StringBuilder();
				String name = FixGenericName(type.IsGenericTypeDefinition ? type.Name : type.GetGenericTypeDefinition().Name);
				if (full && !String.IsNullOrEmpty(type.Namespace))
					sb.Append(type.Namespace).Append(".");
				sb.Append(name);
				sb.Append(BuildTemplateParamsString(type.GetGenericArguments()));
				return prefix + sb.ToString();
			}

            return full ? type.FullName : prefix + type.Name;
        }

		string GetConstructorName(System.Type type)
		{
			if (type.IsGenericTypeDefinition)
				return FixGenericName(type.Name);
			if (type.IsGenericType)
				return GetConstructorName(type.GetGenericTypeDefinition());
			return type.Name;
		}

        public string TrimTypeName(String name, String defaultNamespace, params String[] otherNamespaces)
        {
            //if (name == null)
            //    return null;
            //if (defaultNamespace != null && name.StartsWith(defaultNamespace))
            //    return name.Substring(defaultNamespace.Length + (defaultNamespace.EndsWith(".") ? 0 : 1));
            //if (otherNamespaces != null)
            //    foreach (var ns in otherNamespaces)
            //        if (name.StartsWith(ns))
            //            return name.Substring(ns.Length);
            return name;
        }

        private MemberFlag GetMemberFlags(FieldInfo info)
        {
            MemberFlag res = MemberFlag.NONE;
            if (info.IsLiteral)
                res |= MemberFlag.CONST;
            else if (info.IsStatic)
                res |= MemberFlag.STATIC;
            return res;
        }

        private MemberFlag GetMemberFlags(MethodBase info)
        {
            MemberFlag res = MemberFlag.NONE;
            if (info.IsConstructor)
                res |= MemberFlag.CONSTRUCTOR;
            if (info.IsStatic)
                res |= MemberFlag.STATIC;
            if (info.IsAbstract)
                res |= MemberFlag.ABSTRACT;
            else if (info.IsVirtual)
            {
                res |= MemberFlag.VIRTUAL;
                if (((MethodInfo)info).GetBaseDefinition().DeclaringType != info.DeclaringType)
                    res |= MemberFlag.OVERRIDEN;
            }
            return res;
        }

        private ProtectionLevel ExtractProtectionLevel(PropertyInfo property)
        {
            if (property.CanRead)
                return ExtractProtectionLevel(property.GetGetMethod() ?? property.GetSetMethod());
            return ProtectionLevel.PUBLIC;
        }

        private ProtectionLevel ExtractProtectionLevel(MethodBase methodInfo)
        {
            if (methodInfo.IsPrivate)
                return ProtectionLevel.PRIVATE;
            else if (methodInfo.IsFamilyAndAssembly)
                return ProtectionLevel.INTERNAL | ProtectionLevel.PROTECTED;
            else if (methodInfo.IsFamily)
                return ProtectionLevel.PROTECTED;
            else if (methodInfo.IsPublic)
                return ProtectionLevel.PUBLIC;
            return ProtectionLevel.PUBLIC;
        }

        Type GetType(String fullName, bool create = false)
        {
			var searchName = fullName.Replace('+', '.'); // for nested type
            Type res;
            if (typeCache.TryGetValue(searchName, out res))
                return res;
            
			var lastDot = fullName.LastIndexOf('.');

            res = new Type
            {
                Name = lastDot == -1 ? fullName : fullName.Substring(lastDot + 1),				
                Namespace = lastDot == -1 ? null : fullName.Substring(0, lastDot)
            };

			if (create)
				typeCache.Add(searchName, res);
            return res;
        }

        private Field GetField(string fullName)
        {
            var dotIndex = fullName.LastIndexOf('.');
            if (dotIndex == -1)
                throw new InvalidOperationException(String.Format("Invalid field name '{0}'.", fullName));
            var typeName = fullName.Substring(0, dotIndex);
            var fieldName = fullName.Substring(dotIndex + 1);
            var type = GetType(typeName);
            if (type.Fields == null)
                type.Fields = new List<Field>();
            var field = type.Fields.Find(a => a.RefId == fieldName);
            if (field == null)
            {
                field = new Field { Name = fieldName, RefId = fieldName };
                //type.Fields.Add(field);
            }
            return field;
        }

        private Event GetEvent(string fullName)
        {
            var dotIndex = fullName.LastIndexOf('.');
            if (dotIndex == -1)
                throw new InvalidOperationException(String.Format("Invalid event name '{0}'.", fullName));
            var typeName = fullName.Substring(0, dotIndex);
            var eventName = fullName.Substring(dotIndex + 1);
            var type = GetType(typeName);
            if (type.Events == null)
                type.Events = new List<Event>();
            var ev = type.Events.Find(a => a.RefId == eventName);
            if (ev == null)
            {
                ev = new Event { Name = eventName, RefId = eventName };
                //type.Events.Add(ev);
            }
            return ev;
        }

        private Property GetProperty(string fullName)
        {
            var name = Util.ExtractMethodName(fullName); //Indexer properties have signature like methods
            var dotIndex = name.LastIndexOf('.');
            if (dotIndex == -1)
                throw new InvalidOperationException(String.Format("Invalid property name '{0}'.", fullName));
            var typeName = fullName.Substring(0, dotIndex);
            var fieldName = fullName.Substring(dotIndex + 1);
            var type = GetType(typeName);
            if (type.Properties == null)
                type.Properties = new List<Property>();
            var property = type.Properties.Find(a => a.RefId == fieldName);
            if (property == null)
            {
                property = new Property { Name = fieldName, RefId = fieldName };
                //type.Properties.Add(property);
            }
            return property;
        }

        Method GetMethod(String fullName)
        {
            var signatureIndex = fullName.IndexOf('(');
            if (signatureIndex == -1)
                fullName += "()";
            var dotIndex = fullName.LastIndexOf('.', signatureIndex == -1 ? fullName.Length - 1 : signatureIndex - 1);
            if (dotIndex == -1)
                throw new InvalidOperationException(String.Format("Invalid method name '{0}'.", fullName));
            var typeName = fullName.Substring(0, dotIndex);
            var FullMethodName = fullName.Substring(dotIndex + 1).Replace("@", "");
            var type = GetType(typeName);
            if (type.Methods == null)
                type.Methods = new List<Method>();
            var method = type.Methods.Find(a => a.RefId == FullMethodName);
            if (method == null)
            {
                var refId = FullMethodName;
                if (FullMethodName.StartsWith("#ctor"))
                    FullMethodName = FullMethodName.Replace("#ctor", Util.ExtractTemplateName(type.Name));
                method = new Method
                {
                    Name = FixGenericName(Util.ExtractMethodName(FullMethodName)),
                    RefId = refId
                };
                //type.Methods.Add(method);
            }
            return method;
        }

        private string FixGenericMethodName(string methodName)
        {
            int tind = methodName.IndexOf('`');
            if (tind != -1)
            {
                var bind = methodName.IndexOf('(');
                if (bind != -1)
                    return methodName.Substring(0, tind) + methodName.Substring(bind);
                else
                    return methodName.Substring(0, tind);
            }
            return methodName;
        }

        private string FixGenericName(string name)
        {
            int i = name.IndexOf('`');
            if (i != -1)
                return name.Substring(0, i);
            return name;
        }
    }

    static class Extensions
    {
        public static String SafeGetValue(this XAttribute a) { return a != null ? a.Value : null; }

        public static String SafeGetDescendantValue(this XElement element, String descendant)
        {
            if (element == null)
                return null;
            var el = element.Descendants(descendant).FirstOrDefault();
            if (el != null)
                return el.Value;
            return null;
        }

        public static void ParseSummary(this CodeElement element, XElement node)
        {
            if (node == null)
                return;
            var snode = node.Descendants("summary").FirstOrDefault();
            if (snode == null)
                return;
            var tree = snode.LoadDescription();
            if (tree != null)
            {
                if (tree.Count == 1 && tree[0].Type == DescriptionSegmentType.Text)
                    element.Description = tree[0].Text;
                else
                    element.DescriptionTree = tree;
            }
        }

        public static void ParseDescription(this CodeElement element, XElement snode)
        {
            if (snode == null)
                return;
            var tree = snode.LoadDescription();
            if (tree != null)
            {
                if (tree.Count == 1 && tree[0].Type == DescriptionSegmentType.Text)
                    element.Description = tree[0].Text;
                else
                    element.DescriptionTree = tree;
            }
        }

        public static List<DescriptionSegment> LoadDescription(this XElement element)
        {
            if (element == null)
                return null;

            List<DescriptionSegment> segments = new List<DescriptionSegment>();

            var elements = element.Nodes().ToArray();
            foreach (var n in element.Nodes())
            {
                switch (n.NodeType)
                {
                    case XmlNodeType.Element:
                        var el = (XElement)n;
                        switch (el.Name.LocalName)
                        {
                            case "para":
                                segments.Add(new DescriptionSegment
                                {
                                    Type = DescriptionSegmentType.Paragraph,
                                    Children = el.LoadDescription()
                                });
                                break;
                            case "h1":
                            case "h2":
                            case "h3":
                            case "h4":
                            case "h5":
                                segments.Add(new DescriptionSegment
                                {
                                    Type = DescriptionSegmentType.Header,
                                    Text = el.Value
                                });
                                break;
                            case "b":
                                segments.Add(new DescriptionSegment
                                {
                                    Type = DescriptionSegmentType.Bold,
                                    Text = el.Value
                                });
                                break;
                            case "strong":
                                segments.Add(new DescriptionSegment
                                {
                                    Type = DescriptionSegmentType.Strong,
                                    Children = el.LoadDescription(),
                                });
                                break;
                            case "i":
                                segments.Add(new DescriptionSegment
                                {
                                    Type = DescriptionSegmentType.Italic,
                                    Text = el.Value
                                });
                                break;
                            case "html":
                                segments.Add(new DescriptionSegment
                                {
                                    Type = DescriptionSegmentType.Html,
                                    Text = el.Value
                                });
                                break;
                            case "example":
                                segments.Add(new DescriptionSegment
                                {
                                    Type = DescriptionSegmentType.Example,
                                    Children = el.LoadDescription()
                                });
                                break;
                            case "see":
                                segments.Add(ParseSee(DescriptionSegmentType.See, el.Attribute("cref").SafeGetValue()));
                                break;
                            case "seealso":
                                segments.Add(ParseSee(DescriptionSegmentType.SeeAlso, el.Attribute("cref").SafeGetValue()));
                                break;
                            case "code":
                                segments.Add(new DescriptionSegment
                                {
                                    Type = DescriptionSegmentType.Code,
                                    Text = DeindentCode(el.Value)
                                });
                                break;
                        }
                        break;
                    case XmlNodeType.Text:
                        var xtext = (XText)n;
                        segments.Add(new DescriptionSegment { Text = xtext.Value, Type = DescriptionSegmentType.Text });
                        break;
                }
            }

            if (segments.Count == 0)
                return null;
            return segments;
        }

        private static string DeindentCode(string code)
        {
            if (String.IsNullOrEmpty(code))
                return code;
            var lines = code.Split('\n').Select(a => a.TrimEnd('\r', ' ')).SkipWhile(a => a.Trim() == String.Empty).ToArray();
            int min = int.MaxValue;
            foreach (var l in lines)
                if (l.Length > 0)
                {
                    int c = 0;
                    while (c < l.Length && l[c] == ' ')
                        c++;
                    if (c < min)
                        min = c;
                }
            if (min == 0 || min == int.MaxValue)
                return code;
            StringBuilder sb = new StringBuilder();
            int empty = 0;
            foreach (var l in lines)
            {
                if (l == String.Empty)
                    empty++;
                else
                {
                    for (; empty > 0; empty--)
                        sb.AppendLine();
                    sb.AppendLine(l.Substring(min).Trim('\r'));
                }
            }
            return sb.ToString();
        }

        private static DescriptionSegment ParseSee(DescriptionSegmentType type, string cref)
        {
            if (cref.Length > 2 && cref[1] == ':')
            {
                var res = cref.Substring(2);
                var refv = res;
                switch (cref[0])
                {
                    case 'P':
                    case 'F':
                    case 'M':
                        var ld = res.LastIndexOf('.');
                        if (ld != -1)
                            refv = res.Substring(0, ld) + "-" + res.Substring(ld + 1);
                        return new DescriptionSegment
                        {
                            Type = type,
                            Text = res,
                            Ref = refv
                        };
                    case 'T':
                        return new DescriptionSegment
                        {
                            Type = type,
                            Text = res,
                            Ref = refv
                        };
                }
            }
            return new DescriptionSegment { Type = type, Text = cref, Ref = cref };
        }
    }
}