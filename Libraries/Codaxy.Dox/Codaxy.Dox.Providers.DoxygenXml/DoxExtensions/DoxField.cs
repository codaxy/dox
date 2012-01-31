using System;
using DoxDoxygenXML.Model.Doxygen;
using System.Collections.Generic;
using Codaxy.Dox;
namespace DoxDoxygenXML.DoxExtensions
{
	/// <summary>
	/// Parse extension methods for the Dox.Field class. Extension are related
	/// to the DoxygenXML model definition in (Model.Doxygen). 
	/// </summary>
	public static class DoxField
	{
		
		/// <summary>
		/// Read Dox.Field information from the DoxygenXML model object, by
		/// parsing the related (xml) sections. 
		/// </summary>
		/// <param name="field">
		/// A <see cref="Codaxy.Dox.Field"/>
		/// </param>
		/// <param name="mdt">
		/// A <see cref="memberdefType"/>
		/// </param>	
		public static void  ReadDoxygenMember (this Codaxy.Dox.Field field, memberdefType mdt)
		{
			if (field.RefId.Equals(mdt.id))
			{
				// desc
				if (field.DescriptionTree == null) field.DescriptionTree = new List<DescriptionSegment>();
					mdt.briefdescription.GetDescription(field.DescriptionTree);
					mdt.detaileddescription.GetDescription(field.DescriptionTree);
				
				// Protection
				if (mdt.protSpecified)
					field.ProtectionLevel = ParserTools.ParseProtectionLevel(mdt.prot.ToString());
				
				
				// Property return type
				if (mdt.type != null && mdt.type.Text != null){
					// check for ovveride in type error 
					if (mdt.type.Text[0].StartsWith("override")){
						mdt.type.Text[0] = mdt.type.Text[0].TrimStart("override".ToCharArray());	
						field.MemberFlag |= MemberFlag.OVERRIDEN;
					}
					field.TypeName = mdt.type.Text[0];
				}
				
			}
			
		}
		
	}
}

