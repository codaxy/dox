using System;
using System.Collections.Generic;
using Codaxy.Common.Logging;

namespace Codaxy.Dox.Providers
{
    public class XmlCommentsDoxProvider : IDoxProvider
    {
        List<string> sourceFiles;

        public XmlCommentsDoxProvider()
        {
            sourceFiles = new List<string>();
        }

        public void AddSource(string path)
        {
            sourceFiles.Add(path);
        }

        public Logger Logger { get; set; }

        public void Process(IDoxBuilder builder)
        {
            if (builder == null)
                throw new ArgumentException("builder");

            using (var impl = new XmlCommentsDoxProviderImpl(builder, sourceFiles) { Logger = Logger })
            {
                impl.Process();
            }
        }
    }
}