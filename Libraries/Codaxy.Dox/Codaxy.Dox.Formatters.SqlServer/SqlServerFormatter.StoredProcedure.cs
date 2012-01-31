using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using Codaxy.Common.Web.Html;
using Codaxy.Dox.Formatters.SqlServer;

namespace Codaxy.Dox.Formatters
{
    public partial class SqlServerFormatter
    {
        private void WriteStoredProcedureDocument(StoredProcedure sp, HtmlBuilder html, DatabaseContext context)
        {
            html.h1(String.Format("Stored Procedure: {0}", sp.Name));

            html.e("table");
            html.tr().td().attCls("label").text("Database:").c().td();
            String relDoc = String.Format("{0}.{1}.{1}", context.BookName, context.DbName);
            html.internalLink(context.DbName, "db-entity-name", relDoc, sp.Name);
            html.c().c();
            html.c(); //table

            html.h2("Parameters");
            if (sp.Parameters != null && sp.Parameters.Count > 0)
            {
                html.table(0, 0).att("width", "100%").attCls("members");
                html.dbSpParamHeader();

                foreach (var p in sp.Parameters)
                {
                    html.tr()
                        .rowHeader(false)
                        .td()
                            .attCls(p.IsOutputParameter ? "sp-parameter output" : "sp-parameter input")
                            .att("title", p.IsOutputParameter ? "output" : "input")
                            .text(p.Name.TrimStart('@'))
                        .c()
                        .td()
                            .dataType(p.DataType)
                        .c()
                    .c();
                }

                html.c(); // table
            }
            else
                html.text("This stored procedure does not receive parameters.");
        }

        private static void RenderParametersSection(IEnumerable<StoredProcedureParameter> parameters, HtmlBuilder html)
        {
            html.div().attCls("section");

            html.h4("Parameters");
            if (parameters.Count() > 0)
            {
                html.e("ul").attCls("sp-parameters");
                foreach (var p in parameters)
                    html.e("li").text(p.Name.TrimStart('@')).c();
                html.c(); // ul
            }
            else
                html.text("This stored procedure does not receive parameters.");

            html.c(); // section
        }
    }
}
