using System;
using Codaxy.Dox;
namespace DoxDoxygenXML
{	
	public class ParserTools
	{

		/// <summary>
		/// Split the name from the fullName
		/// </summary>
		/// <param name="name">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public static string GetNameFromFullName(string name)
		{
			int pos = name.LastIndexOf(':');
			if (pos > 0)
				return name.Substring(pos+1);
			else
				return name;
				
		}
		
		/// <summary>
		/// Split the Namepsace information from the fullName 
		/// </summary>
		/// <param name="fullName">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public static string GetNamespaceFromFullName(string fullName)
		{
			int pos = fullName.LastIndexOf(':');
			if (pos > 0)
				return fullName.Substring(0,pos-1);
			else
				return "Global";
		}
		
				/// <summary>
		/// Parse protection level from string to ProtectionLevel enum
		/// </summary>
		/// <param name="level">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="ProtectionLevel"/>
		/// </returns>
		public static ProtectionLevel ParseProtectionLevel(string level)
		{
			switch(level)
			{
				case "private":
					return Codaxy.Dox.ProtectionLevel.PRIVATE;
				case "public":
					return Codaxy.Dox.ProtectionLevel.PUBLIC;
				case "internal":
					return Codaxy.Dox.ProtectionLevel.INTERNAL;
				case "protected":
					return Codaxy.Dox.ProtectionLevel.PROTECTED;
			}
			return Codaxy.Dox.ProtectionLevel.PUBLIC;
		}

		/// <summary>
		/// Parse type of Type from string to Type Enum
		/// </summary>
		/// <param name="kind">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="Codaxy.Dox.Type"/>
		/// </returns>
		public static Codaxy.Dox.Type CreateType(string kind)
        {
            switch (kind)
            {
                case "class":
                    return new Codaxy.Dox.Type(){ TypeKind = Codaxy.Dox.TypeKind.CLASS };
                case "interface":
                    return new Codaxy.Dox.Type(){ TypeKind = Codaxy.Dox.TypeKind.INTERFACE};
                case "delegate":
                    return new Codaxy.Dox.Type(){ TypeKind = Codaxy.Dox.TypeKind.DELEGATE};
                case "enum":
                    return new Codaxy.Dox.Type(){ TypeKind = Codaxy.Dox.TypeKind.ENUM};
                case "struct":
                    return new Codaxy.Dox.Type(){ TypeKind = Codaxy.Dox.TypeKind.STRUCT};
            }
            return new Codaxy.Dox.Type();
        }
	
	}
}

