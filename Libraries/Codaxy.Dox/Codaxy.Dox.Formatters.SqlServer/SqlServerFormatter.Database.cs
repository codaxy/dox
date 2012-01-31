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
        private void WriteDatabaseDocument(Database db, HtmlBuilder html, DatabaseContext context)
        {
            html.h1(String.Format("Database: {0}", db.Name));

            #region Tables 

            html.e("a").attCls("tables").c();
            html.h2("Tables");
            if (db.Tables == null || db.Tables.Count == 0)
                html.text("This database has no tables.");
            else
            {
                html.table(0, 0).att("width", "100%").attCls("members");
                html.dbTableHeader();

                foreach (var t in db.Tables)
                {
                    html.tr().attCls("expandable");
                    html.td().attCls("micon").e("a").attCls("exi").att("href", "#expand").nbsp().c().c();
                    String relDoc = String.Format("{0}.{1}.Tables.{2}.{3}", context.BookName, context.DbName, t.SchemaName, t.Name);
                    html.td().internalLink(t.Name, "db-entity-name " + t.Name, relDoc); // Anchor and link in one element (e.g. <a class="db-entity-name Parties" ...>)
                    html.div().attCls("details");
                    RenderColumnsSection(t.Columns, html);
                    html.c(); // details
                    html.c(); // td
                    html.td().text(t.SchemaName).c();
                    html.c(); // tr
                }

                html.c(); // table
            }

            #endregion
            
            #region Views

            html.e("a").attCls("views").c();
            html.h2("Views");
            if (db.Views == null || db.Views.Count == 0)
                html.text("This database has no views.");
            else
            {
                html.table(0, 0).att("width", "100%").attCls("members");
                html.dbTableHeader();

                foreach (var v in db.Views)
                {
                    html.tr().attCls("expandable");
                    html.td().attCls("micon").e("a").attCls("exi").att("href", "#expand").nbsp().c().c();
                    String relDoc = String.Format("{0}.{1}.Views.{2}.{3}", context.BookName, context.DbName, v.SchemaName, v.Name);
                    html.td().internalLink(v.Name, "db-entity-name " + v.Name, relDoc);
                    html.div().attCls("details");
                    RenderColumnsSection(v.Columns, html);
                    html.c(); // details
                    html.c(); // td

                    html.td().text(v.SchemaName).c();

                    html.c(); // tr
                }

                html.c(); // table
            }

            #endregion

            #region Stored procedures

            html.e("a").attCls("stored-procedures").c();
            html.h2("Stored procedures");
            if (db.StoredProcedures == null || db.StoredProcedures.Count == 0)
                html.text("This database has no stored procedures.");
            else
            {
                html.table(0, 0).att("width", "100%").attCls("members");
                html.dbSpHeader();

                foreach (var sp in db.StoredProcedures)
                {
                    html.tr().attCls("expandable");
                    html.td().attCls("micon").e("a").attCls("exi").att("href", "#expand").nbsp().c().c();

                    String relDoc = String.Format("{0}.{1}.Procedures.{2}", context.BookName, context.DbName, sp.Name);
                    html.td().internalLink(sp.Name, "db-entity-name " + sp.Name, relDoc);

                    html.div().attCls("details");
                    RenderParametersSection(sp.Parameters, html);
                    html.c(); // details

                    html.c(); // td
                    html.c(); // tr
                }

                html.c(); // table
            }

            #endregion
        }
    }
}
