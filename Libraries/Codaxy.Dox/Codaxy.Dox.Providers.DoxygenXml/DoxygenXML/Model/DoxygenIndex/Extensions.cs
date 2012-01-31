using System;
using System.Collections.Generic;
using Codaxy.Dox;
namespace DoxDoxygenXML.Model.DoxygenIndex
{
	/// <summary>
	/// Extension methods in types of DoxygenIndex	
	/// </summary>
	public static class Extensions
	{

		/// <summary>
		/// Loop through each DoxygenType and gather its information.
		/// Build from the generated Dox.Type, Dox.Documents with the 
		/// Namespace.
		/// </summary>
		/// <param name="t">
		/// A <see cref="DoxygenType"/>
		/// </param>
		/// <returns>
		/// A <see cref="List<DoxDocument>"/>
		/// </returns>
		public static List<DoxDocument> GetDoxTree(this DoxygenType t)
		{
			List<DoxDocument> DoxTree = new List<DoxDocument> ();
			foreach (CompoundType compound in t.compound) {
				foreach (Codaxy.Dox.Type type in compound.GetDoxType ()) {
					if (type != null) {
						type.Namespace = type.Namespace.Replace("::",".");
						DoxTree.Add (new DoxDocument { 
							FullName = type.FullName.Replace("::","."),
							Title = type.Name,
							Type = type 
						});
					}
				}
			}
			return DoxTree;
		}
			
		
		/// <summary>
		/// Parses the DoxygenType information for an existing Dox.Type definition.
		/// And creates the Dox.Type. In last step the DoxygenTypeMember information
		/// is parsed and added to the Dox.Type.
		/// </summary>
		/// <param name="t">
		/// A <see cref="CompoundType"/>
		/// </param>
		/// <returns>
		/// A <see cref="List<Dox.Type>"/>
		/// </returns>
		public static List<Codaxy.Dox.Type> GetDoxType (this CompoundType t)
		{
			List<Codaxy.Dox.Type> types = new List<Codaxy.Dox.Type> ();
			
			string nSpace = ParserTools.GetNamespaceFromFullName(t.name);
			Codaxy.Dox.Type type = null;
			switch (t.kind) {
			case CompoundKind.@namespace:
				nSpace = t.name;
			break;
			case CompoundKind.@class:
				type = new Codaxy.Dox.Type {
					Namespace = nSpace,
					Name = ParserTools.GetNameFromFullName(t.name),
					RefId = t.refid,
					TypeKind = TypeKind.CLASS
				};
				types.Add (type);
				break;
			case CompoundKind.@struct:
				type = new Codaxy.Dox.Type {
					Namespace = nSpace,
					Name = ParserTools.GetNameFromFullName(t.name),
					RefId = t.refid,
					TypeKind = TypeKind.STRUCT
				};
				types.Add(type);
			break;
			case CompoundKind.file:
				type = new Codaxy.Dox.Type {
					Name = ParserTools.GetNameFromFullName(t.name),
					Namespace = ParserTools.GetNameFromFullName(t.name),
					RefId = t.refid
					//TypeKind = TypeKind.FILE
				};
			break;
			case CompoundKind.dir:
			
			break;
			}
			if (t.member != null)
				foreach (MemberType member in t.member) {
					member.GetMember (type, types, nSpace);
				}
			
			return types;
		}
		
		/// <summary>
		/// Parse the DoxygenMember information. Creates a Member if existing in 
		/// Dox.Member definition and add it to the given Dox.Type.
		/// </summary>
		/// <param name="m">
		/// A <see cref="MemberType"/>
		/// </param>
		/// <param name="type">
		/// A <see cref="Dox.Type"/>
		/// </param>
		/// <param name="types">
		/// A <see cref="List<Dox.Type>"/>
		/// </param>
		/// <param name="nSpace">
		/// A <see cref="System.String"/>
		/// </param>
		public static void GetMember (this MemberType m, Codaxy.Dox.Type type, List<Codaxy.Dox.Type> types, string nSpace)
		{
			switch (m.kind) {
			case MemberKind.@enum:
				types.Add (new Codaxy.Dox.Type { 
					Name = m.name, 
					TypeKind = TypeKind.ENUM, 
					Namespace = nSpace,
					RefId = m.refid 
				});
				break;
			case MemberKind.enumvalue:
				foreach (Codaxy.Dox.Type _type in types) {
					if (_type.RefId.Equals (m.refid)) {
						if (_type.Fields == null)
							_type.Fields = new List<Field> ();
						_type.Fields.Add (new Field { Name = m.name, RefId = m.refid });
					}
				}
				break;
			case MemberKind.function:
				if (type == null) {	// so it is namespace (delegate)
					types.Add (type = new Codaxy.Dox.Type {
						Namespace = nSpace,
						Name = ParserTools.GetNameFromFullName(m.name),
						RefId = m.refid 
					});
				} else {
					// so it is a member method
					if (type.Methods == null)
						type.Methods = new List<Method> ();
						type.Methods.Add (new Codaxy.Dox.Method { 
							Name = m.name, 
							RefId = m.refid,
							DeclaringType = type.Name
						});
				}
				break;
			case MemberKind.variable:
				if (type == null)
					System.Console.WriteLine("");
				if (type.Fields == null)
					type.Fields = new List<Field> ();
					type.Fields.Add (new Codaxy.Dox.Field { 
						Name = ParserTools.GetNameFromFullName(m.name),
						RefId = m.refid 
				});
				break;
			case MemberKind.property:
				if (type != null)
				if (type.Properties == null)
					type.Properties = new List<Property>();
				type.Properties.Add(new Codaxy.Dox.Property {
					Name = ParserTools.GetNameFromFullName(m.name),
					RefId = m.refid
				});
				break;
			}
			
		}
		
	}
}

