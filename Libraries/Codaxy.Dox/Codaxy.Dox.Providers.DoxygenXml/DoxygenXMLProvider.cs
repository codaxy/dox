using System;
using Codaxy.Dox;
using Codaxy.Common.Logging;
using Codaxy.Common.Text;
using DoxDoxygenXML;

namespace Codaxy.Dox.Providers
{
	public class DoxygenXMLProvider : IDoxProvider
	{	
		public Logger Logger { get; set; }
			
		private string path;
		
		public DoxygenXMLProvider (string path, Logger logger)	
		{
			this.path = path;
		}
		
		public void Process(IDoxBuilder builder) {
			foreach(Codaxy.Dox.DoxDocument doc in Parser.GetDocumentList(path)){
				builder.Add(doc);
			}
		}
		
	}
}

