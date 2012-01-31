using System;
using System.Collections.Generic;
using System.IO;
using Codaxy.Common.Logging;

namespace Codaxy.Dox.DoxZipFile.Builder
{
    class Program
    {
        enum Provider { Undefined, XmlComments, Doxygen, Sql}

        static string outputFile;
        static Logger consoleLog;

        static void Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("Dox file generation utility.");
            Console.WriteLine();

            consoleLog = new Logger(new ConsoleLogAppender(), "");

            try
            {
                if (args.Length == 0)
                {
                    PrintUsage();
                    return;
                }

                Provider provider = Provider.Undefined;
                var sourceFiles = new List<string>();
                foreach (var arg in args)
                {
                    if (arg.StartsWith("--p:"))
                        provider = ParseProvider(arg);
                    else if (arg.StartsWith("--o:"))
                        outputFile = arg.Substring(4);
                    else if (arg.StartsWith("--i:"))
                        sourceFiles.Add(arg.Substring(4));
                }

                if (outputFile == null)
                    throw new InvalidOperationException("Output file not specified!");

                if (sourceFiles.Count == 0)
                    throw new InvalidOperationException("Sources not specified!");

                switch (provider)
                {
                    case Provider.XmlComments:
                        ProcessXmlComments(sourceFiles);
                        break;
                    case Provider.Doxygen:
                        ProcessDoxygenXml(sourceFiles);
                        break;
                    case Provider.Sql:
                        ProcessSqlDoc("", "");
                        break;
                    default:
                        throw new InvalidOperationException("Provider not specified!");
                }

                Console.WriteLine("Success!");
            }
            catch (Exception eX)
            {
                Console.Error.WriteLine("Exception ocurred. Terminating!");
                Console.Error.WriteLine(eX.ToString());
            }
            finally
            {
                // Console.ReadKey();
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  --p:provider \tAllowed values are xml|doxygen|sql.");
            Console.WriteLine("  --o:output \tPath to output file.");
            Console.WriteLine("  --i:input \tPath to source file. Use this switch multiple times for multiple sources.");
            Console.WriteLine();
        }

        private static Provider ParseProvider(string arg)
        {
            var split = arg.Split(':');
            var p = split[1].ToLower();
			System.Console.WriteLine("----------------"+p);
            switch (p)
            {
                case "xml": return Provider.XmlComments;
                case "doxygen": return Provider.Doxygen;
                case "sql": return Provider.Sql;
            }
            throw new InvalidOperationException(String.Format("Unsupported provider {0}.", p));
        }

        private static void ProcessXmlComments(List<string> sourceFiles)
        {
            var provider = new Providers.XmlCommentsDoxProvider() { Logger = consoleLog.ChildLogger("xml-comments") };
            foreach (var source in sourceFiles)
                provider.AddSource(source);

            using (var fs = File.Create(outputFile))
            using (var zipOutput = new DoxZipFileWriter(fs))
            {
                provider.Process(zipOutput);
            }
        }

        private static void ProcessDoxygenXml(List<string> pathToXmlFiles)
        {
            var provider = new Codaxy.Dox.Providers.DoxygenXMLProvider(pathToXmlFiles[0], consoleLog.ChildLogger("xml-comments"));
					
		   using (var fs = File.Create(outputFile))
		   using (var zipOutput = new DoxZipFileWriter(fs))
		   {
				provider.Process(zipOutput);		
		   }
        }

        private static void ProcessSqlDoc(string connectionString, string databaseName)
        {
            var provider = new Codaxy.Dox.Providers.SqlProvider(connectionString, databaseName) { Logger = consoleLog.ChildLogger("provider") };

            using (var fs = File.Create(outputFile))
            using (var zipOutput = new DoxZipFileWriter(fs))
            {
                provider.Process(zipOutput);
            }
        }

       
    }
}