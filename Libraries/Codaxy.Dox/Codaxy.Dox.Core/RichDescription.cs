using System.Collections.Generic;

namespace Codaxy.Dox
{
    public enum DescriptionSegmentType { Text, Html, Code, See, SeeAlso, Paragraph, Example, Bold, Strong, Italic, Header }

    public class DescriptionSegment
    {
        public DescriptionSegmentType Type { get; set; }

        public string Text { get; set; }

        public string Ref { get; set; }

        public List<DescriptionSegment> Children { get; set; }

        public void AddChild(DescriptionSegment s)
        {
            if (Children == null)
                Children = new List<DescriptionSegment>();
            Children.Add(s);
        }
    }
}