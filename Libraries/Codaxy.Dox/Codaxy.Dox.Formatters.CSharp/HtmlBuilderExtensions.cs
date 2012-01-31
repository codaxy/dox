using System;
using System.Linq;
using Codaxy.Common.Text;

namespace Codaxy.Dox.Formatters.CSharp
{
    class ReferenceContext
    {
        public string BookName { get; set; }

        public string Namespace { get; set; }
    }

    static class HtmlBuilderExtensions
    {
        public static HtmlBuilder protectionLevel(this HtmlBuilder builder, ProtectionLevel plevel)
        {
            switch (plevel)
            {
                case ProtectionLevel.PRIVATE: return builder.text("private");
                case ProtectionLevel.INTERNAL: return builder.text("internal"); ;
                case ProtectionLevel.PROTECTED: return builder.text("protected");
                default:
                case ProtectionLevel.PUBLIC: return builder.text("public");
            }
        }

        public static HtmlBuilder typeName(this HtmlBuilder builder, string typeName, ReferenceContext referenceContext)
        {
            return builder.typeName(typeName, typeName, referenceContext);
        }

        public static HtmlBuilder typeName(this HtmlBuilder builder, string typeName, string reference, ReferenceContext referenceContext)
        {
            return builder.typeName(true, typeName, reference, referenceContext);
        }

        public static HtmlBuilder typeName(this HtmlBuilder builder, bool codetag, string typeName, string reference, ReferenceContext referenceContext)
        {
            if (String.IsNullOrEmpty(typeName))
                return builder;

            //if (typeName == reference)
            {
                var parts = typeName.SplitKeep('<', '>', ' ', ',', '[', ']').ToArray();
                if (parts.Length > 1)
                {
                    builder.code(codetag);
                    for (var i = 0; i < parts.Length; i++)
                    {
                        var part = parts[i];
                        if (part.Length > 0)
                            if (Char.IsLetter(part[0]))
                                builder.typeName(false, part, i == 0 ? reference : part, referenceContext);
                            else
                                builder.text(part);
                    }

                    return builder.c(codetag);//code
                }
            }

            var shortName = CSharpUtil.TrimTypeName(typeName);

            if (shortName == "Void")
                shortName = "void";

            if (typeName.StartsWith("System."))
                return builder.code(codetag).e("a").attCls("type-name").att("href", "http://msdn.microsoft.com/en-us/library/" + typeName.LeftOf('<', false) + ".aspx").att("target", "_blank").text(shortName).c().c(codetag);

            var href = referenceContext.BookName + "." + reference;
            string doc = null;
            string section = null;
            if (href != null)
            {
                var hrefPart = href.Split('-');
                if (hrefPart.Length > 0)
                    doc = hrefPart[0];
                if (hrefPart.Length > 1)
                    section = hrefPart[1];
            }
            builder.code(codetag).e("a").attCls("type-name").att("href", "#" + doc);
            if (!string.IsNullOrEmpty(doc))
                builder.att("dox:doc", doc);
            if (!string.IsNullOrEmpty(section))
                builder.att("dox:section", System.Net.WebUtility.HtmlEncode(section));
            builder.text(shortName).c().c(codetag);
            return builder;
        }

        public static HtmlBuilder paramName(this HtmlBuilder builder, string paramName)
        {
            return builder.span().attCls("param-name").text(paramName).c();
        }

        public static HtmlBuilder memberName(this HtmlBuilder builder, string memberName)
        {
            return builder.span().attCls("member-name").text(memberName).c();
        }

        public static HtmlBuilder anchor(this HtmlBuilder builder, string id)
        {
            return builder.e("a").att("class", System.Net.WebUtility.HtmlEncode(id)).c();
        }

        public static HtmlBuilder member(this HtmlBuilder builder, Member member, ReferenceContext referenceContext)
        {
            return builder.member(member, null, referenceContext);
        }

        public static HtmlBuilder member(this HtmlBuilder builder, Member member, String specialMemberPrefix, ReferenceContext referenceContext)
        {
            return builder.anchor(member.Name).protectionLevel(member.ProtectionLevel).text(" ").memberFlags(member.MemberFlag).text(specialMemberPrefix).typeName(member.TypeName, referenceContext).text(" ").memberName(member.Name);
        }

        public static HtmlBuilder tableHeader(this HtmlBuilder builder)
        {
            return builder.tr().th().att("colspan", "2").text("Name").c().th().text("Defined in").c().c();
        }

        public static HtmlBuilder memberFlags(this HtmlBuilder builder, MemberFlag flags)
        {
            if ((flags & MemberFlag.ABSTRACT) != MemberFlag.NONE)
                builder.text("abstract ");
            if ((flags & MemberFlag.OVERRIDEN) != MemberFlag.NONE)
                builder.text("override ");
            else if ((flags & MemberFlag.VIRTUAL) != MemberFlag.NONE)
                builder.text("virtual ");
            if ((flags & MemberFlag.CONST) != MemberFlag.NONE)
                builder.text("const ");
            if ((flags & MemberFlag.STATIC) != MemberFlag.NONE)
                builder.text("static ");
            return builder;
        }

        public static HtmlBuilder code(this HtmlBuilder builder, bool check)
        {
            if (check)
                builder.e("code");
            return builder;
        }

        public static HtmlBuilder c(this HtmlBuilder builder, bool check)
        {
            if (check)
                builder.c();
            return builder;
        }
    }
}