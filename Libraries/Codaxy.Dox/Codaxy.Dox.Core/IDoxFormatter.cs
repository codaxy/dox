using System;
using System.Collections.Generic;
using System.IO;

namespace Codaxy.Dox
{
    public class DoxFormattingContext
    {
        public String BookName { get; set; }

        public TextWriter Output { get; set; }
    }

    public interface IDoxFormatter
    {
        void Write(DoxDocument dox, DoxFormattingContext outputContext);

        string Name { get; }
    }

    public interface IDoxFormatterFactory
    {
        IDoxFormatter CreateDoxFormatter(string languageCode);
    }

    public class DoxFormatterNotFoundException : Exception
    {
        public DoxFormatterNotFoundException(String msg) : base(msg) { }
    }

    public class DoxFormatterRegistry : IDoxFormatterFactory
    {
        Dictionary<String, IDoxFormatter> formatters;
        IDoxFormatter defaultFormatter;

        public DoxFormatterRegistry()
        {
            formatters = new Dictionary<string, IDoxFormatter>();
        }

        public void Register(IDoxFormatter formatter, bool isDefault)
        {
            if (formatter == null)
                throw new ArgumentNullException("formatter");

            formatters[formatter.Name] = formatter;
            if (isDefault)
                defaultFormatter = formatter;
        }

        public void Register(IDoxFormatter formatter) { Register(formatter, false); }

        public IDoxFormatter CreateDoxFormatter(string languageCode)
        {
            IDoxFormatter res;
            if (languageCode != null && formatters.TryGetValue(languageCode, out res))
                return res;
            if (defaultFormatter != null)
                return defaultFormatter;
            throw new DoxFormatterNotFoundException(String.Format("Formatter for language '{0}' does not exist, and no default formatter specified.", languageCode));
        }
    }
}