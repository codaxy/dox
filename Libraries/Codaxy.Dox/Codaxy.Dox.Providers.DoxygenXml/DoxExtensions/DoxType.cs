using System;
using Codaxy.Dox;
using System.Collections.Generic;
using DoxDoxygenXML.Model.Doxygen;

namespace DoxDoxygenXML.DoxExtensions
{
	/// <summary>
	/// Parse extension methods for the Dox.Type class. Extension are related
	/// to the DoxygenXML model definition in (Model.Doxygen). 
	/// </summary>
	public static class DoxType
	{
		
		/// <summary>
		/// Parse the Dox.Type description from the related DoxygenType object.
		/// In next step loop throug all member information in the DoxygenType object
		/// and parse their information, if given.
		/// </summary>
		/// <param name="type">
		/// A <see cref="Codaxy.Dox.Type"/>
		/// </param>
		/// <param name="dt">
		/// A <see cref="DoxygenType"/>
		/// </param>
		public static void ReadDoxygenType (this Codaxy.Dox.Type type, DoxygenType dt)
		{
			if (type.Name.Equals("AnEnum"))
						Console.WriteLine("");
			
			if (dt.compounddef.Length != 0) {
				compounddefType cft = dt.compounddef[0];
				if (cft.id.Equals (type.RefId)) {
					
					// Base type
					if (cft.basecompoundref != null)
					foreach (compoundRefType baseType in cft.basecompoundref)
					{
						type.AddBaseType(baseType.refid);
					}
				
					// Type Description 
					type.DescriptionTree = new List<DescriptionSegment> ();
					cft.briefdescription.GetDescription(type.DescriptionTree);
					cft.detaileddescription.GetDescription(type.DescriptionTree);

					// loop throug all membery
					if (cft.sectiondef != null)
					foreach (sectiondefType section in cft.sectiondef)
						foreach (memberdefType member in section.memberdef)
						{
							ReadDoxygenMember(type, member);
						}
				}
			}
		}
		
		
		/// <summary>
		/// Loop through all possible Dox.Type members, and parse their information. 
		/// </summary>
		/// <param name="type">
		/// A <see cref="Codaxy.Dox.Type"/>
		/// </param>
		/// <param name="memdefType">
		/// A <see cref="memberdefType"/>
		/// </param>
		public static void ReadDoxygenMember (this Codaxy.Dox.Type type, memberdefType memdefType)
		{
			if (type.Methods != null)
			foreach(Codaxy.Dox.Method method in type.Methods){
				method.ReadDoxygenMember(memdefType);
				method.DeclaringType = type.FullName;
			}
			if (type.Fields != null)
			foreach(Codaxy.Dox.Field field in type.Fields){
				field.ReadDoxygenMember(memdefType);
				field.DeclaringType = type.FullName;
			}
			if (type.Properties != null)
			foreach(Codaxy.Dox.Property property in type.Properties){
				property.ReadDoxygenMember(memdefType);
				property.DeclaringType = type.FullName;
			}
		}
		
		
		/// <summary>
		/// Extenstion method to add a Method to an Dox.Type by pre
		/// checking for the existance of the Method.
		/// </summary>
		/// <param name="type">
		/// A <see cref="Codaxy.Dox.Type"/>
		/// </param>
		/// <param name="method">
		/// A <see cref="Codaxy.Dox.Method"/>
		/// </param>
		public static void AddMethod(this Codaxy.Dox.Type type, Codaxy.Dox.Method method)
		{
			bool Exists = false;
			foreach (Method m in type.Methods)
			{
				if (m.Name.Equals(method.Name))
					Exists = true;
			}
			if (!Exists) type.Methods.Add(method);
		}
		
		/// <summary>
		/// Extenstion method to add a Event to an Dox.Type by pre
		/// checking for the existance of the Event.
		/// </summary>
		/// <param name="type">
		/// A <see cref="Codaxy.Dox.Type"/>
		/// </param>
		/// <param name="_event">
		/// A <see cref="Codaxy.Dox.Event"/>
		/// </param>
		public static void AddEvent(this Codaxy.Dox.Type type, Codaxy.Dox.Event _event)
		{
			bool Exists = false;
			foreach (Event e in type.Events)
			{
				if (e.Name.Equals(_event.Name))
					Exists = true;
			}
			if (!Exists) type.Events.Add(_event);
		}
		
		
		/// <summary>
		/// Extenstion method to add a Property to an Dox.Type by pre
		/// checking for the existance of the Property.
		/// </summary>
		/// <param name="type">
		/// A <see cref="Codaxy.Dox.Type"/>
		/// </param>
		/// <param name="property">
		/// A <see cref="Codaxy.Dox.Property"/>
		/// </param>
		public static void AddProperty(this Codaxy.Dox.Type type, Codaxy.Dox.Property property)
		{
			bool Exists = false;
			foreach (Property p in type.Properties)
			{
				if (p.Name.Equals(property.Name))
					Exists = true;
			}
			if (!Exists) type.Properties.Add(property);
		}
		
		/// <summary>
		/// Extenstion method to add a Field to an Dox.Type by pre
		/// checking for the existance of the Field.
		/// </summary>
		/// <param name="type">
		/// A <see cref="Codaxy.Dox.Type"/>
		/// </param>
		/// <param name="field">
		/// A <see cref="Codaxy.Dox.Field"/>
		/// </param>
		public static void AddFiled( this Codaxy.Dox.Type type, Codaxy.Dox.Field field)
		{
			bool Exists = false;
			foreach (Field f in type.Fields)
			{
				if (f.Name.Equals(field.Name))
					Exists = true;
			}
			if (!Exists) type.Fields.Add(field);
		}
		
		/// <summary>
		/// Updating the Base-Type information of an Dox.Type object.
		/// In first pars step, the type has just the BaseType reference
		/// identifier. In this step the identifier got replaced by the 
		/// related BaseType name.
		/// </summary>
		/// <param name="type">
		/// A <see cref="Codaxy.Dox.Type"/>
		/// </param>
		/// <param name="baseType">
		/// A <see cref="Codaxy.Dox.Type"/>
		/// </param>
		public static void UpdateBaseType(this Codaxy.Dox.Type type, Codaxy.Dox.Type baseType)
		{	
			if (type.BaseTypes != null)
			foreach (String baseRef in type.BaseTypes)		
			{
				if (baseRef.Equals(baseType.RefId))			// found
				{
					// Methods
					if (baseType.Methods != null){
						if (type.Methods ==  null)
							type.Methods = new List<Method>(baseType.Methods);
						else
							foreach (Method method in baseType.Methods)
									type.AddMethod(method);
					}
					// Properties
					if (baseType.Properties != null){
						if (type.Properties ==  null)
							type.Properties = new List<Property>(baseType.Properties);
						else
							foreach (Property property in baseType.Properties)
									type.AddProperty(property);
					}
					// Fields
					if (baseType.Fields != null){
						if (type.Fields ==  null)
							type.Fields = new List<Field>(baseType.Fields);
						else
							foreach (Field field in baseType.Fields)
									type.AddFiled(field);
					}
					// Events
					if (baseType.Events != null){
						if (type.Events ==  null)
							type.Events = new List<Event>(baseType.Events);
						else
							foreach (Event _event in baseType.Events)
									type.AddEvent(_event);
					}
					
					type.BaseTypes.Remove(baseRef);
					type.BaseTypes.Add(baseType.Name);
					break;	
				}
			}
		}
	}
	
}

