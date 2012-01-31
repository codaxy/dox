using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Codaxy.Common.Web.Html;
using Codaxy.Dox.Formatters.SqlServer;
using Codaxy.Common.Text;

namespace Codaxy.Dox.Formatters
{
    public partial class SqlServerFormatter
    {
        private void RenderColumnsTable(IEnumerable<Column> columns, HtmlBuilder html, DatabaseContext context)
        {
            html.h2("Columns");

            html.table(0, 0).att("width", "100%").attCls("members");
            
            html.dbColumnHeader();
            foreach (var c in columns)
            {
                html.tr();
                html.rowHeader(false);

                html.td();
                if (c.IsInPrimaryKey) html.attCls("key primary-key");
                else if (c.IsForeignKey) html.attCls("key foreign-key");
                else html.attCls("key");
                html.text(c.Name);
                if (!c.Descritpion.IsNullOrEmpty())
                {
                    html.div().attCls("summary");
                    html.text(c.Descritpion);
                    html.c(); // summary
                }
                html.c(); // td

                
                html.td();
                html.anchor(c.Name);
                html.dataType(c.DataType);
                html.c();
                
                html.td();

                if (c.ReferenceData != null)
                {
                    string relDoc = String.Format("{0}.{1}.Tables.{2}.{3}", context.BookName, context.DbName, c.ReferenceData.ReferencedTableSchema, c.ReferenceData.ReferencedTable);
                    html.internalLink(String.Format("{0}.{1}", c.ReferenceData.ReferencedTable, c.ReferenceData.ReferencedColumn), "db-entity-name", relDoc, c.ReferenceData.ReferencedColumn);
                }
                else
                    html.nbsp();

                html.c(); // td
                html.c(); // tr
            }

            html.c(); // table
        }

        private static void RenderColumnsSection(IEnumerable<Column> columns, HtmlBuilder html)
        {
            html.div().attCls("section");
            html.h4("Columns");
            html.e("ul").attCls("table-columns");
            foreach (var c in columns)
                html.e("li").text(c.Name).c();

            html.c(); // ul
            html.c(); // section
        }
    }
}
