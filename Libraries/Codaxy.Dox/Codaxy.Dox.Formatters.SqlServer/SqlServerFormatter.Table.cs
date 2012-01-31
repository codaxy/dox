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
        private void WriteTableDocument(Table table, HtmlBuilder html, DatabaseContext context)
        {
            html.h1(String.Format("Table: {0}", table.Name));

            html.e("table");
            html.tr().td().attCls("label").text("Database:").c().td();
            String relDoc = String.Format("{0}.{1}.{1}", context.BookName, context.DbName);
            html.internalLink(context.DbName, "db-entity-name", relDoc, table.Name);
            html.c().c();
            html.c(); //table

            RenderColumnsTable(table.Columns, html, context);
        }
    }
}
