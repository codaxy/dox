using System;
using System.Collections.Generic;
using Codaxy.Dox;
namespace DoxDoxygenXML.Model.Doxygen
{
	public static class Extensions
	{
		
		//---------------------------------------------------------------------------------------
		//------- descriptionType Extension
		//---------------------------------------------------------------------------------------
		
		/// <summary>
		/// Return inner xml text
		/// </summary>
		/// <param name="t">
		/// A <see cref="descriptionType"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public static string GetText(this descriptionType t)
		{
			string _text = "";
			if (t.Text != null)	
				foreach(string text in t.Text)
					_text += text;
			if (t.para != null)
				foreach(var item in t.para)
					_text += item.Text[0];
			return _text;
		}
		
		/// <summary>
		/// Parses children objects for Paramter description, and adds
		/// it to the given Dox.Parameter
		/// </summary>
		/// <param name="t">
		/// A <see cref="descriptionType"/>
		/// </param>
		/// <param name="parameter">
		/// A <see cref="Codaxy.Dox.Parameter"/>
		/// </param>
		public static void GetParameterDescription(this descriptionType t, Codaxy.Dox.Parameter parameter)
		{
			if (t.para != null)
			foreach (docParaType para in t.para)
				if (para.Items != null)
				foreach (var paraList in para.Items)
					if (paraList.GetType().Equals(typeof(docParamListType)))
					foreach (docParamListItem item in ((docParamListType)paraList).parameteritem)					{
						if (item.parameternamelist != null && item.parameternamelist[0].Text != null)
							if (item.parameternamelist[0].Text[0].Equals(parameter.Name)){ // description found
								string desc = item.parameterdescription.GetText();
								parameter.Description = desc;
							} 
								
					}
		}
		
		/// <summary>
		/// Get the return description of an method 
		/// </summary>
		/// <param name="t">
		/// A <see cref="descriptionType"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public static string GetMethodReturnDescription(this descriptionType t)
		{
			if (t.para != null)
			foreach (docParaType para in t.para)
				if (para.Items != null)
				foreach (var paraList in para.Items)
					if (paraList.GetType().Equals(typeof(docSimpleSectType)))
					{
						docSimpleSectType stype = (docSimpleSectType)paraList;
						if (stype.para != null)
							return stype.GetText();
					}		
			return "";
		}
		
		/// <summary>
		/// Generate a Dox.DescriptionTree from the all descritptions in the 
		/// description type
		/// </summary>
		/// <param name="t">
		/// A <see cref="descriptionType"/>
		/// </param>
		/// <param name="doxList">
		/// A <see cref="List<Dox.DescriptionSegment>"/>
		/// </param>
		public static void GetDescription(this descriptionType t, List<Codaxy.Dox.DescriptionSegment> doxList)
		{
			if (t.Text != null && t.Text.Length > 1)
				foreach(string text in t.Text)
					doxList.Add(new DescriptionSegment(){
						Text = text,
						Type = DescriptionSegmentType.Text
					});
			if (t.para != null)
			foreach (docParaType tp in t.para)
				tp.GetDescriptionSegment(doxList);
		}
		
		
		//---------------------------------------------------------------------------------------
		//------- docParaType Extension
		//---------------------------------------------------------------------------------------
		
		/// <summary>
		/// Create a Dox.Descritpion element from information in docParaType
		/// </summary>
		/// <param name="t">
		/// A <see cref="docParaType"/>
		/// </param>
		/// <param name="segmentList">
		/// A <see cref="List<Dox.DescriptionSegment>"/>
		/// </param>
		public static void GetDescriptionSegment (this docParaType t, List<Codaxy.Dox.DescriptionSegment> segmentList)
		{
			
			if (t.Text != null)
			foreach(var text in t.Text)
			{
				segmentList.Add(new Codaxy.Dox.DescriptionSegment()
				{
					Text = text,
					Type = Codaxy.Dox.DescriptionSegmentType.Text
				});
			}
			
			if (t.Items != null)
			foreach (var item in t.Items){
										
			if (item.GetType ().Equals (typeof(docHeadingType))) {
				segmentList.Add(new Codaxy.Dox.DescriptionSegment(){
					Text = ((docHeadingType)item).Text[0],
					Type = Codaxy.Dox.DescriptionSegmentType.Header
				});
			}
			
			if (item.GetType ().Equals (typeof(docRefTextType))) {
				segmentList.Add(new Codaxy.Dox.DescriptionSegment { 
					Text = ((docRefTextType)item).Text[0], 
					Ref = ((docRefTextType)item).Text[0], Type = Codaxy.Dox.DescriptionSegmentType.See 
				});
			}
			
			if (item.GetType ().Equals (typeof(listingType))) {
				listingType type = ((listingType)item);
				var CodeSeg = new Codaxy.Dox.DescriptionSegment ();
				CodeSeg.Type = Codaxy.Dox.DescriptionSegmentType.Code;
				foreach (codelineType line in type.codeline) {
					foreach (highlightType high in line.highlight) {
						if (high.Text != null)
						foreach (string text in high.Text) {
							CodeSeg.Text += text + " ";
						}
					}
				}
				segmentList.Add(CodeSeg);
			}
				
			}

		}
		
		
		/// <summary>
		/// Get the innner (xml) text
		/// </summary>
		/// <param name="t">
		/// A <see cref="docParaType"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public static string GetText(this docParaType t)
		{
			string _text = "";
			if (t.Text != null)	
				foreach(string text in t.Text)
					_text += text;
			return _text;
		}
		
		
		//---------------------------------------------------------------------------------------
		//------- docParaType Extension
		//---------------------------------------------------------------------------------------
		
		
		/// <summary>
		/// Get the inner (xml) text
		/// </summary>
		/// <param name="t">
		/// A <see cref="docSimpleSectType"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public static string GetText(this docSimpleSectType t)
		{
			string _text = "";
			if (t.para != null)
				foreach(docParaType item in t.para)
					if (item.Text != null)
						_text += item.Text[0];
			return _text;
		}
		
		//---------------------------------------------------------------------------------------
		//------- paramType Extension
		//---------------------------------------------------------------------------------------

		
		/// <summary>
		/// Read detailed Descritption from descriptionType and add it to
		/// Dox.Parameter.
		/// </summary>
		/// <param name="t">
		/// A <see cref="paramType"/>
		/// </param>
		/// <param name="detaileddescription">
		/// A <see cref="descriptionType"/>
		/// </param>
		/// <returns>
		/// A <see cref="Codaxy.Dox.Parameter"/>
		/// </returns>
		public static Codaxy.Dox.Parameter GetParam(this paramType t, descriptionType detaileddescription)
		{
			Codaxy.Dox.Parameter param = new Codaxy.Dox.Parameter();

			// Type
			if (t.type != null && t.type.Text != null)
				param.TypeName = t.type.Text[0];
			
			// Name
			if (t.declname != null){
				System.Xml.XmlNode[] nodes = ((System.Xml.XmlNode[])t.declname);
				foreach(var node in nodes)
					param.Name = node.Value.ToString();
			}
			
			detaileddescription.GetParameterDescription(param);
		
			return param;
		}
		
		
	}
}

