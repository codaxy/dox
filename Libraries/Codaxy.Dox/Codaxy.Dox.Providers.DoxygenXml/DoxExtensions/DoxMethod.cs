using System;
using DoxDoxygenXML.Model.Doxygen;
using System.Collections.Generic;
using Codaxy.Dox;
namespace DoxDoxygenXML.DoxExtensions
{
	
	/// <summary>
	///	Parse extension methods for the Dox.Method class. Extension are related
	/// to the DoxygenXML model definition in (Model.Doxygen). 
	/// </summary>
	public static class DoxMethod
	{
		
	
		/// <summary>
		/// Read Dox.Field information from the DoxygenXML model object, by
		/// parsing the related (xml) sections. 
		/// </summary>
		/// <param name="method">
		/// A <see cref="Codaxy.Dox.Method"/>
		/// </param>
		/// <param name="mdt">
		/// A <see cref="memberdefType"/>
		/// </param>
		public static void ReadDoxygenMember (this Codaxy.Dox.Method method , memberdefType mdt)
		{
			// Existing method and new method information 
			if (method.RefId.Equals(mdt.id)){  
							
				// desc
				if (method.DescriptionTree == null) method.DescriptionTree = new List<DescriptionSegment>();
				mdt.briefdescription.GetDescription(method.DescriptionTree);
				mdt.detaileddescription.GetDescription(method.DescriptionTree);
				method.ReturnDescription = mdt.detaileddescription.GetMethodReturnDescription();
				
				// Protection
				if (mdt.protSpecified)
				method.ProtectionLevel = ParserTools.ParseProtectionLevel(mdt.prot.ToString());
				
				// Exception
				//if (mdt.exceptions != null)
				//method.Exception = mdt.exceptions.Text.ToString();
				
				// Method flags
				if (mdt.@static.Equals(DoxBool.yes)) method.MemberFlag |= MemberFlag.STATIC;
				if (mdt.virtSpecified && mdt.virt.Equals(DoxVirtualKind.@virtual))
					method.MemberFlag |= MemberFlag.VIRTUAL;
			
				// Method return type
				if (mdt.type != null && mdt.type.Text != null){
					// check for ovveride in type error 
					if (mdt.type.Text[0].StartsWith("override")){
						mdt.type.Text[0] = mdt.type.Text[0].TrimStart("override".ToCharArray());	
						method.MemberFlag |= MemberFlag.OVERRIDEN;
					}
					method.TypeName = mdt.type.Text[0];
				}
				
				// Method params
				if (mdt.param != null){
					if (method.Parameters == null) method.Parameters = new List<Parameter>();
					foreach(paramType para in mdt.param)
						method.Parameters.Add(para.GetParam(mdt.detaileddescription));
				}
				
			}	
		}
		
	}
}

