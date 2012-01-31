using System;
using DoxDoxygenXML.Model.Doxygen;
using System.Collections.Generic;
using Codaxy.Dox;
namespace DoxDoxygenXML.DoxExtensions
{
	
	/// <summary>
	/// Parse extension methods for the Dox.Property class. Extension are related
	/// to the DoxygenXML model definition in (Model.Doxygen). 
	/// </summary>
	public static class DoxProperty
	{
	
	
		/// <summary>
		/// Read Dox.Propety information from the DoxygenXML model object, by
		/// parsing the related (xml) sections. 
		/// </summary>
		/// <param name="property">
		/// A <see cref="Codaxy.Dox.Property"/>
		/// </param>
		/// <param name="mdt">
		/// A <see cref="memberdefType"/>
		/// </param>
		public static void ReadDoxygenMember (this Codaxy.Dox.Property property, memberdefType mdt)
		{
			if (property.RefId.Equals(mdt.id))
			{
				
				// desc
				if (property.DescriptionTree == null) property.DescriptionTree = new List<DescriptionSegment>();
					mdt.briefdescription.GetDescription(property.DescriptionTree);
					mdt.detaileddescription.GetDescription(property.DescriptionTree);
				
				// Protection
				if (mdt.protSpecified)
					property.ProtectionLevel = ParserTools.ParseProtectionLevel(mdt.prot.ToString());
				
				// Property return type
				if (mdt.type != null && mdt.type.Text != null){
					// check for ovveride in type error 
					if (mdt.type.Text[0].StartsWith("override")){
						mdt.type.Text[0] = mdt.type.Text[0].TrimStart("override".ToCharArray());	
						property.MemberFlag |= MemberFlag.OVERRIDEN;
					}
					property.TypeName = mdt.type.Text[0];
				}
				
			}	
		}
	}
}

