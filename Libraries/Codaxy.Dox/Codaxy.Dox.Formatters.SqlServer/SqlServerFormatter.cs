using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using Codaxy.Dox.Formatters.SqlServer;

namespace Codaxy.Dox.Formatters
{
    public partial class SqlServerFormatter : IDoxFormatter
    {
        public void Write(DoxDocument dox, DoxFormattingContext outputContext)
        {
            var output = outputContext.Output;

            using (var hw = new HtmlTextWriter(output))
            {
                var html = new HtmlBuilder(hw);
                html.div().attCls("dox").att("dox:format", "sql");

                if (dox.DbElement != null)
                {
                    var context = new DatabaseContext
                    {
                        BookName = outputContext.BookName,
                        DbName = dox.DbElement.DbName
                    };

                    if (dox.DbElement.Database != null)
                        WriteDatabaseDocument(dox.DbElement.Database, html, context);

                    else if (dox.DbElement.Table != null)
                        WriteTableDocument(dox.DbElement.Table, html, context);

                    else if (dox.DbElement.View != null)
                        WriteViewDocument(dox.DbElement.View, html, context);

                    else if (dox.DbElement.StoredProcedure != null)
                        WriteStoredProcedureDocument(dox.DbElement.StoredProcedure, html, context);
                }

                html.c(); // dox div
            }
        }

        public string Name
        {
            get { return "sql"; }
        }
    }
}
