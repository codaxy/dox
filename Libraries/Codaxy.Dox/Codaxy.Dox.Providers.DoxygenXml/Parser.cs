using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using DoxDoxygenXML.Model.DoxygenIndex;
using DoxDoxygenXML.DoxExtensions;
namespace DoxDoxygenXML
{
	public class Parser
	{
		public static List<Codaxy.Dox.DoxDocument> GetDocumentList(string path)
		{
			
			//string path = "/home/mihailo/quark/blse/Dox/Dox.Viewer/demo/DoxygenXML/";
			//string path = "/home/mihailo/Desktop/basisapp/DOC/xml/";
			
			// Directory to the Doxygen XML Doc
			DirectoryInfo DocPath = new DirectoryInfo (path);
							
			// Create an instance of the XmlSerializer specifying type and namespace.
     		XmlSerializer serializer = new XmlSerializer(typeof(DoxygenType));

      		// A FileStream is needed to read the XML document.
      		//FileStream fs = new FileStream("/home/mihailo/quark/blse/Dox/Dox.Viewer/demo/DoxygenXML/index.xml", FileMode.Open);
			FileStream fs = new FileStream(path + "index.xml", FileMode.Open);
      		XmlReader reader = new XmlTextReader(fs);
          
      		// Declare an object variable of the type to be deserialized.
      		DoxygenType t = (DoxygenType) serializer.Deserialize(reader);
		
			List<Codaxy.Dox.DoxDocument> DoxTree = t.GetDoxTree();
			
			// Read Compound		
			// Create XML document and load the index.xml file
			serializer = new XmlSerializer(typeof(DoxDoxygenXML.Model.Doxygen.DoxygenType));
			List<FileInfo> Files = new List<FileInfo> (DocPath.GetFiles ("*.xml"));
			
			int errors = 0;
			foreach(FileInfo file in Files)
			{
				fs = new FileStream(file.FullName, FileMode.Open);
				reader = new XmlTextReader(fs);
				try{
					DoxDoxygenXML.Model.Doxygen.DoxygenType comp = (DoxDoxygenXML.Model.Doxygen.DoxygenType) serializer.Deserialize(reader);
					foreach (Codaxy.Dox.DoxDocument doc in DoxTree)
					{		
						doc.Type.ReadDoxygenType(comp);
					}
				} catch (InvalidOperationException ex){
					errors++;
					System.Console.Error.WriteLine(ex.Message);
				}
			}
			
			// Update baseType information 
			foreach (Codaxy.Dox.DoxDocument doc in DoxTree)
			{
				Codaxy.Dox.Type type = doc.Type;				// The type to be updated	
				foreach (Codaxy.Dox.DoxDocument baseDoc in DoxTree)
				{
					Codaxy.Dox.Type baseType = baseDoc.Type;
					type.UpdateBaseType(baseType);
				}
			}
			
			return DoxTree;
			
		}
	
	}
	
	

}

