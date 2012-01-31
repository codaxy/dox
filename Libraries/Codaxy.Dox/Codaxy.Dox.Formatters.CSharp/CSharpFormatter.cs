using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Codaxy.Dox.Formatters.CSharp;

namespace Codaxy.Dox.Formatters
{
    public class CSharpFormatter : IDoxFormatter
    {
        public void Write(DoxDocument dox, DoxFormattingContext outputContext)
        {
            var output = outputContext.Output;
            using (var hw = new HtmlTextWriter(output))
            {
                var html = new HtmlBuilder(hw);
                html.div().attCls("dox").att("dox:format", "class");

                if (dox.Type != null)
                {
                    html.e("h1");
                    switch (dox.Type.TypeKind)
                    {
                        case TypeKind.CLASS:
                            html.text("class ");
                            break;
                        case TypeKind.ENUM:
                            html.text("enum ");
                            break;
                        case TypeKind.INTERFACE:
                            html.text("interface ");
                            break;
                        case TypeKind.STRUCT:
                            html.text("struct ");
                            break;
                        case TypeKind.DELEGATE:
                            html.text("delegate ");
                            break;
                    }

                    html.text(dox.Type.Name);
                    html.c();//h1;
                    var referenceContext = new ReferenceContext
                    {
                        BookName = outputContext.BookName,
                        Namespace = dox.Type.Namespace
                    };

                    html.e("table");
                    html.tr().td().attCls("label").text("Namespace:").c().td().text(dox.Type.Namespace).c().c();
                    html.tr().td().attCls("label").text("Base:").c().td();
                    if (dox.Type.BaseTypes != null)
                    {
                        bool first = true;
                        foreach (var bt in dox.Type.BaseTypes)
                        {
                            if (first)
                                first = false;
                            else html.text(", ");
                            html.typeName(bt, referenceContext);
                        }
                    }
                    html.c().c();
                    html.c(); //table

                    RenderExtendedSummary(dox.Type, html, referenceContext);

                    #region Fields

                    html.anchor("fields");
                    html.h2("Fields");
                    if (dox.Type.Fields == null || dox.Type.Fields.Count == 0)
                        html.text("This class has no fields.");
                    else
                    {
                        html.table(0, 0).att("width", "100%").attCls("members");
                        html.tableHeader();
                        foreach (var m in dox.Type.Fields.OrderBy(a => a.Name))
                        {
                            html.tr();//.attCls("expandable");
                            //html.td().attCls("micon").e("a").attCls("exi").att("href", "#expand").nbsp().c().c();
                            html.td().attCls("micon").nbsp().c();

                            html.td().member(m, referenceContext);
                            if (m.InitLiteral != null)
                                html.text(" = ").text(m.InitLiteral);
                            //html.div().attCls("details");
                            RenderExtendedSummary(m, html, referenceContext);
                            //html.c(); // details;
                            html.c(); //td
                            html.td().att("width", "10%").typeName(m.DeclaringType, m.DeclaringType + "-" + m.Name, referenceContext).c();
                            html.c(); //tr
                        }
                        html.c(); //table
                    }

                    #endregion Fields

                    #region Properties

                    html.e("a").attCls("properties").c();
                    html.h2("Properties");
                    if (dox.Type.Properties == null || dox.Type.Properties.Count == 0)
                        html.text("This class has no properties.");
                    else
                    {
                        html.table(0, 0).att("width", "100%").attCls("members");
                        html.tableHeader();
                        foreach (var m in dox.Type.Properties.OrderBy(a => a.Name))
                        {
                            html.tr();

                            if (m.Parameters != null) //indexers
                            {
                                html.attCls("expandable");
                                html.td().attCls("micon").e("a").attCls("exi").att("href", "#expand").nbsp().c().c();
                            }
                            else
                                html.td().attCls("micon").nbsp().c();

                            html.td().member(m, referenceContext);
                            if (m.Parameters != null)
                            {
                                html.text("[");
                                RenderParameters(html, m.Parameters, referenceContext);
                                html.text("]");
                            }
                            html.text(" { ");
                            if (m.CanRead) html.text("get; ");
                            if (m.CanWrite) html.text("set; ");
                            html.text("}");
                            RenderExtendedSummary(m, html, referenceContext);

                            if (m.Parameters != null)
                            {
                                html.div().attCls("details");
                                RenderMethodParametersAndReturnsSummary(html, m, referenceContext);
                                html.c(); // details
                            }

                            html.c(); //td
                            html.td().att("width", "10%").typeName(m.DeclaringType, m.DeclaringType + "-" + m.Name, referenceContext).c();
                            html.c(); //tr
                        }
                        html.c();
                    }

                    #endregion Properties

                    #region Methods


                    html.anchor("methods");
                    html.h2("Methods");
                    if (dox.Type.Methods == null || dox.Type.Methods.Count == 0)
                        html.text("This class has no methods.");
                    else
                    {
                        html.table(0, 0).att("width", "100%").attCls("members");
                        html.tableHeader();
                        foreach (var m in dox.Type.Methods.OrderByDescending(a => a.MemberFlag & MemberFlag.CONSTRUCTOR).ThenBy(a => a.Name))
                        {
                            html.tr();
                            if (m.Parameters != null || (m.TypeName != null && m.TypeName != "System.Void" && m.TypeName != "void"))
                            {
                                html.attCls("expandable");
                                html.td().attCls("micon").e("a").attCls("exi").att("href", "#expand").nbsp().c().c();
                            }
                            else
                                html.td().attCls("micon").nbsp().c();

                            html.td().member(m, referenceContext);
                            html.text("(");
                            RenderParameters(html, m.Parameters, referenceContext);
                            html.text(")");
                            RenderExtendedSummary(m, html, referenceContext);
                            html.div().attCls("details");
                            RenderMethodParametersAndReturnsSummary(html, m, referenceContext);
                            html.c(); // details
                            html.c(); //td
                            html.td().att("width", "10%").typeName(m.DeclaringType, m.DeclaringType + "-" + m.Name, referenceContext).c();
                            html.c(); //tr
                        }
                        html.c();
                    }

                    #endregion Methods

                    #region Events

                    html.anchor("events");
                    html.h2("Events");
                    if (dox.Type.Events == null || dox.Type.Events.Count == 0)
                        html.text("This class has no events.");
                    else
                    {
                        html.table(0, 0).att("width", "100%").attCls("members");
                        html.tableHeader();
                        foreach (var m in dox.Type.Events.OrderBy(a => a.Name))
                        {
                            html.tr();//.attCls("expandable");
                            //html.td().attCls("micon").e("a").attCls("exi").att("href", "#expand").nbsp().c().c();
                            html.td().attCls("micon").nbsp().c();

                            html.td().member(m, "event ", referenceContext);

                            //html.div().attCls("details");
                            RenderExtendedSummary(m, html, referenceContext);
                            //html.c(); // details;
                            html.c(); //td
                            html.td().att("width", "10%").typeName(m.DeclaringType, m.DeclaringType + "-" + m.Name, referenceContext).c();
                            html.c(); //tr
                        }
                        html.c(); //table
                    }

                    #endregion Events
                }
                else
                    html.h1(dox.Title);

                //html.div().attCls("footer").text(dox.Title).c();
                html.c(); // dox div
            }
        }

        private void RenderMethodParametersAndReturnsSummary(HtmlBuilder html, Method m, ReferenceContext referenceContext)
        {
            if (m.Parameters != null && m.Parameters.Count > 0)
            {
                html.div().attCls("section");
                html.h4("Parameters:");
                html.e("ul").attCls("method-params");
                foreach (var p in m.Parameters)
                {
                    html.e("li").paramName(p.Name).text(": ").typeName(p.TypeName, referenceContext);
                    RenderExtendedSummary(p, html, referenceContext);
                    html.c(); //li
                }
                html.c(); //ul
                html.c(); //div
            }

            if (m.TypeName != null && m.TypeName != "Void" && m.TypeName != "System.Void")
            {
                html.div().attCls("section");
                html.h4("Returns:");
                html.e("ul").attCls("method-params")
                    .e("li").typeName(m.TypeName, referenceContext);
                RenderSummary(m.ReturnDescription, html, referenceContext);
                html.c();//c
                html.c(); //ul
                html.c(); //div
            }
        }

        private void RenderParameters(HtmlBuilder html, List<Parameter> parameters, ReferenceContext referenceContext)
        {
            if (parameters != null)
            {
                bool first = true;
                foreach (var p in parameters)
                {
                    if (first)
                        first = false;
                    else
                        html.text(", ");
                    if ((p.ParameterFlags & ParameterFlags.Const) != ParameterFlags.None)
                        html.text("const ");
                    if ((p.ParameterFlags & ParameterFlags.Out) != ParameterFlags.None)
                        html.text("out ");
                    if ((p.ParameterFlags & ParameterFlags.Ref) != ParameterFlags.None)
                        html.text("ref ");
                    if ((p.ParameterFlags & ParameterFlags.This) != ParameterFlags.None)
                        html.text("this ");
                    html.typeName(p.TypeName, referenceContext).text(" ").paramName(p.Name);
                }
            }
        }

        private void RenderExtendedSummary(CodeElement element, HtmlBuilder html, ReferenceContext referenceContext)
        {
            if (element == null || (element.Description == null && element.DescriptionTree == null))
                return;

            html.div().attCls("summary");
            html.text(element.Description);
            RenderExtendedSummary(element.DescriptionTree, html, referenceContext);
            html.c();//div
        }

        private void RenderSummary(string summary, HtmlBuilder html, ReferenceContext referenceContext)
        {
            if (string.IsNullOrEmpty(summary))
                return;

            html.div().attCls("summary");
            html.text(summary);
            html.c();//div
        }

        private void RenderExtendedSummary(List<DescriptionSegment> list, HtmlBuilder html, ReferenceContext referenceContext)
        {
            if (list == null)
                return;
            foreach (var el in list)
                RenderDescriptionNode(el, html, referenceContext);
        }

        private void RenderDescriptionNode(DescriptionSegment el, HtmlBuilder html, ReferenceContext referenceContext)
        {
            switch (el.Type)
            {
                case DescriptionSegmentType.Text:
                    html.text(el.Text);
                    break;
                case DescriptionSegmentType.SeeAlso:
                case DescriptionSegmentType.See:
                    html.typeName(el.Text, el.Ref, referenceContext);
                    break;
                case DescriptionSegmentType.Paragraph:
                    html.p();
                    html.text(el.Text);
                    RenderExtendedSummary(el.Children, html, referenceContext);
                    html.c();
                    break;
                case DescriptionSegmentType.Bold:
                    html.e("b").text(el.Text);
                    RenderExtendedSummary(el.Children, html, referenceContext);
                    html.c();
                    break;
                case DescriptionSegmentType.Header:
                    html.h4(el.Text);
                    break;
                case DescriptionSegmentType.Italic:
                    html.e("i").text(el.Text);
                    RenderExtendedSummary(el.Children, html, referenceContext);
                    html.c();
                    break;
                case DescriptionSegmentType.Strong:
                    html.e("strong").text(el.Text);
                    RenderExtendedSummary(el.Children, html, referenceContext);
                    html.c();
                    break;
                case DescriptionSegmentType.Example:
                    html.div().attCls("example");
                    html.text(el.Text);
                    RenderExtendedSummary(el.Children, html, referenceContext);
                    html.c();
                    break;
                case DescriptionSegmentType.Code:
                    html.e("pre");
                    html.text(el.Text);
                    RenderExtendedSummary(el.Children, html, referenceContext);
                    html.c();
                    break;
                case DescriptionSegmentType.Html:
                    html.text(el.Text, false);
                    break;
            }
        }

        public string Name
        {
            get { return "cs"; }
        }
    }
}