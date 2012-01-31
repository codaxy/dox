using System;
using System.Linq;
using Codaxy.Common.Text;

namespace Codaxy.Dox.Formatters.SqlServer
{
    class DatabaseContext
    {
        public string BookName { get; set; }
        public string DbName { get; set; }
    }
    
    public static class HtmlBuilderExtensions
    {
        public static HtmlBuilder dbTableHeader(this HtmlBuilder builder)
        {
            return builder
                .tr()
                    .th()
                        .att("colspan", "2")
                        .text("Name")
                    .c()
                    .th()
                        .att("width", "60px")
                        .text("Schema")
                    .c()
                .c();
        }

        public static HtmlBuilder dbSpHeader(this HtmlBuilder builder)
        {
            return builder
                .tr()
                    .th()
                        .att("colspan", "2")
                        .text("Name")
                    .c()
                .c();
        }

        public static HtmlBuilder dbColumnHeader(this HtmlBuilder builder)
        {
            return builder
                .tr()
                    .th().att("colspan", "2").text("Name").c()
                    .th().text("Type").c()
                    .th().text("Foreign Key").c()
                .c();
        }

        public static HtmlBuilder dbSpParamHeader(this HtmlBuilder builder)
        {
            return builder
                .tr()
                    .th().att("colspan", "2").text("Name").c()
                    .th().text("Type").c()
                .c();
        }

        public static HtmlBuilder internalLink(this HtmlBuilder builder, String text, String cls, String relDoc)
        {
            return internalLink(builder, text, cls, relDoc, null);
        }

        public static HtmlBuilder internalLink(this HtmlBuilder builder, String text, String cls, String relDoc, String section)
        {
            String url = "#" + relDoc;

            builder
                .e("a")
                .att("href", url)
                .att("dox:doc", relDoc)
                .attCls(cls);

            if (!section.IsNullOrEmpty())
                builder.att("dox:section", section);

            return builder.text(text).c();
        }

        public static HtmlBuilder anchor(this HtmlBuilder builder, string id)
        {
            return builder.e("a").att("class", id).c();
        }

        public static HtmlBuilder rowHeader(this HtmlBuilder builder, bool expandable)
        {
            builder.td().attCls("micon");
            if (expandable)
                builder.e("a").attCls("exi").att("href", "#expand").nbsp().c();
            else 
                builder.nbsp();

            return builder.c();
        }

        public static HtmlBuilder dataType(this HtmlBuilder html, DataType dt)
        {
            html.text(dt.Name);

            if (dt.NumericPrecision.HasValue && dt.NumericScale.HasValue)
                html.text(String.Format("({0},{1})", dt.NumericPrecision, dt.NumericScale));
            else if (dt.MaximumLength > 0)
                html.text(String.Format("({0})", dt.MaximumLength));
            else if (dt.MaximumLength.HasValue) // && == 0
                html.text("(MAX)");

            return html;
        }
    }
}
