using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Dynamic;
using System.IO;
using Codaxy.Dextop;
using Codaxy.Dextop.Remoting;
using Codaxy.Dox.Engine;
using Codaxy.Common.Text;
using Codaxy.Common.Logging;

namespace Codaxy.Dox.Viewer
{
    public class Session : DextopSession
    {
        public Logger Logger { get; set; }
        public Application Application { get; set; }

        public Session(Application application)
        {
            Application = application;
        }

        public override void InitRemotable(DextopRemote remote, DextopConfig config)
        {
            base.InitRemotable(remote, config);
        }
    
        class CachedFile
        {
            public String FileName { get; set; }

            public String Content { get; set; }
        }

        void CacheFile(object file)
        {
            var f = file as CachedFile;
            if (f != null)
                try
                {
                    File.WriteAllText(f.FileName, f.Content);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Error caching document file '{0}'.", f.FileName);
                    Logger.Exception(ex);
                }
        }

        [DextopRemotable]
        public String GetDoxDocumentHtml(String url)
        {
            try
            {
                var index = url.IndexOf('.');
                if (index == -1)
                    throw new InvalidOperationException("Invalid url format!");

                var book = Application.Library.GetBook(url.Substring(0, index));
                var cachedFileName = DextopUtil.MapPath("~/cache/" + url + ".htm");
                foreach (var c in Path.GetInvalidPathChars())
                    cachedFileName = cachedFileName.Replace(c, '-');
                var cacheFileInfo = new FileInfo(cachedFileName);

#if !DEBUG
                if (cacheFileInfo.Exists && cacheFileInfo.LastWriteTime > book.LastChangeTime)
                    return File.ReadAllText(cacheFileInfo.FullName);
#endif

                var doxDocument = book.GetDoxDocument(url.Substring(index + 1));

                var formatter = Application.Formatters.CreateDoxFormatter(doxDocument.FormatCode);

                using (var sw = new StringWriter())
                {
                    var outputContext = new DoxFormattingContext
                    {
                        Output = sw,
                        BookName = book.Name
                    };
                    formatter.Write(doxDocument, outputContext);
                    var content = sw.ToString();
                    ThreadPool.QueueUserWorkItem(CacheFile, new CachedFile { FileName = cachedFileName, Content = content });
                    return content;
                }
            }
            catch (DoxDocumentNotFoundException)
            {
                throw new DextopErrorMessageException("Requested document not found on the server!");
            }
            catch (DoxBookNotFoundException)
            {
                throw new DextopErrorMessageException("Requested book not found on the server!");
            }
            catch (DoxFormatterNotFoundException fex)
            {
                throw new DextopErrorMessageException(fex.Message);
            }
            catch (Exception ex)
            {
                throw new DextopErrorMessageException("Exception: {0}", ex.ToString());
            }
        }

        [DextopRemotable]
        public object Search(string pattern)
        {
            var result = new List<object>();

            if (pattern.IsNullOrEmpty() || pattern.Length < 2)
                return result;

            foreach (var book in Application.Library.GetBooks())
                try
                {
                    result.AddRange(book.Search(pattern).Select(a =>
                        new
                        {
                            sectionType = GetSectionType(a.SectionType),
                            section = a.SectionName,
                            description = a.SectionDescription,
                            document = a.DocumentTitle,
                            url = String.Format("{0}.{1}", book.Name, a.DocumentFullName.Split('-')[0])
                        }).ToArray());
                }
                catch
                {
                    // Some books might not support searching, just skip those
                }

            return result;
        }

        public String GetSectionType(DoxSectionType type)
        {
            switch (type)
            {
                case DoxSectionType.Class:
                    return "document";
                case DoxSectionType.Field:
                    return "field";
                case DoxSectionType.Property:
                    return "property";
                case DoxSectionType.Method:
                    return "method";
                case DoxSectionType.Database:
                    return "database";
                case DoxSectionType.Table:
                    return "table";
                case DoxSectionType.View:
                    return "view";
                case DoxSectionType.StoredProcedure:
                    return "stored-procedure";
                case DoxSectionType.Column:
                    return "column";

                default:
                    return "text";
            }
        }
    }
}