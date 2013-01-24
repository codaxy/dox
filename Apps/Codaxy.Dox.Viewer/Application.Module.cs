using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Codaxy.Dextop;
using Codaxy.Dextop.Remoting;
using Codaxy.Dextop.Data;
using Codaxy.Dextop.Forms;

namespace Codaxy.Dox.Viewer
{
    public class ApplicationModule : DextopModule
    {
        public override string ModuleName
        {
            get { return "demo"; }
        }

        protected override void InitNamespaces()
        {
			RegisterNamespaceMapping("Codaxy.Dox.Viewer.Windows*", "Dox.window");
            RegisterNamespaceMapping("Codaxy.Dox.Viewer*", "Dox");                 
        }

        protected override void InitResources()
        {            
            RegisterJs("app", "client/js/",
                "generated/",
                "controls/",
                "");
            
            //RegisterCss("client/themes/resources/css/carbon/carbon.css");
            //RegisterCss("client/themes/resources/css/azzurra/azzurra-core.css");
            //RegisterCss("client/themes/resources/css/azzurra/azzurra-ui-all.css");
            RegisterCss("client/css/application.css");
            RegisterCss("client/css/dox.css");
        }


        protected override void RegisterAssemblyPreprocessors(Dictionary<string, IDextopAssemblyPreprocessor> preprocessors)
        {
            RegisterStandardAssemblyPreprocessors("client/js/generated/", preprocessors);
        }

        protected override void RegisterLoaders(Dictionary<string, IDextopFileLoader> loaders)
        {
            
        }
    }
}