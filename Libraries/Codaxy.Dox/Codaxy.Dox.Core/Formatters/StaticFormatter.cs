using System;

namespace Codaxy.Dox.Formatters
{
    public class StaticFormatter : IDoxFormatter
    {
        public String Text { get; set; }

        public string Name { get; private set; }

        public StaticFormatter(String name) { Name = name; }

        public void Write(DoxDocument dox, DoxFormattingContext outputContext)
        {
            outputContext.Output.Write(Text);
        }
    }
}