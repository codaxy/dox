using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using Codaxy.Dox.Formatters.SqlServer;

namespace Codaxy.Dox.Formatters
{
    public partial class SqlServerFormatter
    {
        private void WriteViewDocument(View view, HtmlBuilder html, DatabaseContext context)
        {
            html.h1(String.Format("View: {0}", view.Name));

            html.e("table");
            html.tr().td().attCls("label").text("Database:").c().td();
            String relDoc = String.Format("{0}.{1}.{1}", context.BookName, context.DbName);
            html.internalLink(context.DbName, "db-entity-name", relDoc, view.Name);
            html.c().c();
            html.c(); //table

            RenderColumnsTable(view.Columns, html, context);
        }
    }
}
