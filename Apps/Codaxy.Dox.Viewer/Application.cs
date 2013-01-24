using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Codaxy.Dextop;
using Codaxy.Dox.Engine;
using Codaxy.Common.Logging;
using System.IO;
using Codaxy.Dextop.Data;

namespace Codaxy.Dox.Viewer
{
    public partial class Application : DextopApplication
    {
        public Logger Logger { get; private set; }
        public DoxLibrary Library { get; private set; }
        public DoxFormatterRegistry Formatters { get; private set; }

        public Application(Logger logger)
        {
            Logger = logger;
 
            Formatters = new DoxFormatterRegistry();
            LoadFormatters();

            Library = new DoxLibrary();
            LoadBooks();

            CreateBooksTreeCache();

            InstallBookWatcher();
        }

        protected override void RegisterModules()
        {
            RegisterModule("http://dextop.codaxy.com/ext/ext-4.1.1a/", new DextopExtJSModule
            {
                Debug = true,
                UsingExternalResources = true,
                SkipCss = false
            });

            RegisterModule("client/lib/dextop", new DextopCoreModule());
            RegisterModule("", new ApplicationModule());
        }

        protected override void OnModulesInitialized()
        {
            base.OnModulesInitialized();
#if !DEBUG
            OptimizeModules("client/js/cache");
#endif
        }


        #region Book watcher

        FileSystemWatcher bookFilesWatcher;

        private void InstallBookWatcher()
        {
            bookFilesWatcher = new FileSystemWatcher(DextopUtil.MapPath("~/books"), "*.dox")
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = false
            };

            bookFilesWatcher.Created += OnDoxFileChanged;
            bookFilesWatcher.Changed += OnDoxFileChanged;
            bookFilesWatcher.Renamed += OnDoxFileChanged;
            bookFilesWatcher.Deleted += OnDoxFileChanged;
        }

        void OnDoxFileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                switch (e.ChangeType)
                {
                    case WatcherChangeTypes.Created:
                    case WatcherChangeTypes.Changed:
                        Logger.InfoFormat("Loading book from file '{0}'.", e.FullPath);
                        Library.AddBook(LoadBookFromFile(e.FullPath));
                        break;
                    case WatcherChangeTypes.Renamed:
                        Logger.InfoFormat("Book file '{0}' renamed. Reloading books.", e.FullPath);
                        LoadBooks();
                        break;
                    case WatcherChangeTypes.Deleted:
                        Logger.InfoFormat("Book file '{0}' deleted. Unloading book.", e.FullPath);
                        Library.RemoveBook(GetBookNameFromFileName(e.Name));
                        break;
                }

                CreateBooksTreeCache();
            }
            catch (Exception ex)
            {
                Logger.Exception("Dox book change processing error.", ex);
            }
        }

        #endregion Book watcher

        #region Tree cache

        [DextopModel]
        public class TreeNode
        {
            [DextopModelId]
            public String id { get; set; }
            public String text { get; set; }
            public String iconCls { get; set; }
            public String cls { get; set; }
            public bool leaf { get; set; }
            public bool expanded { get; set; }
            public TreeNode[] children { get; set; }
            public String type { get; set; }
            public object tag { get; set; }
            public String qtip { get; set; }
        }

        public void CreateBooksTreeCache()
        {
            List<TreeNode> nodes = new List<TreeNode>();

            foreach (var book in Library.GetBooks())
            {
                nodes.Add(new TreeNode
                {
                    id = String.Format("{0}.", book.Name),
                    text = System.Net.WebUtility.HtmlEncode(book.Title),
                    iconCls = "book-node",
                    type = "book",
                    children = GetChapterSubTree(book, null)
                });
            }

            System.IO.File.WriteAllText(DextopUtil.MapPath("~/client/js/generated/data.js"),
                String.Format("Ext.namespace('Dox.Application'); Dox.Application.booksTree = {0};", DextopUtil.Encode(nodes)));            
        }

        private TreeNode[] GetChapterSubTree(Engine.IDoxBook book, String chapterUrl)
        {
            var chapterNodes = book.GetSubChaptersOf(chapterUrl).OrderBy(a => a.Title).Select(a =>
                new TreeNode
                {
                    id = String.Format("{0}.{1}", book.Name, a.Url),
                    text = System.Net.WebUtility.HtmlEncode(a.Title),
                    iconCls = "chapter-node",
                    children = GetChapterSubTree(book, a.Url),
                    type = "chapter"
                });

            var documentNodes = book.GetChapterDocumentsOf(chapterUrl).OrderBy(a => a.Title).Select(a =>
                new TreeNode
                {
                    id = String.Format("{0}.{1}", book.Name, a.Url),
                    text = System.Net.WebUtility.HtmlEncode(a.Title),
                    iconCls = "document-node",
                    leaf = true,
                    type = "document"
                });

            List<TreeNode> result = new List<TreeNode>();
            HashSet<String> idHash = new HashSet<string>();
            foreach (var node in chapterNodes.Union(documentNodes))
            {
                if (idHash.Contains(node.id))
                    Logger.WarningFormat("Duplicate id: {0}", node.id);
                else
                {
                    result.Add(node);
                    idHash.Add(node.id);
                }
            }

            return result.ToArray();
        }

        #endregion Tree cache

        #region Formatters

        private void LoadFormatters()
        {
            Formatters.Register(new Formatters.CSharpFormatter(), true);
            Formatters.Register(new Formatters.SqlServerFormatter(), true);
        }

        #endregion Formatters

        #region Library operations

        private void LoadBooks()
        {
            Library.Clear();

            var booksDir = new DirectoryInfo(DextopUtil.MapPath("~/books"));

            foreach (var f in booksDir.GetFiles("*.dox"))
            {
                Logger.InfoFormat("Loading book from file '{0}'.", f.FullName);
                var book = LoadBookFromFile(f);
                Library.AddBook(book);
            }
        }

        private IDoxBook LoadBookFromFile(String fileName) { return LoadBookFromFile(new FileInfo(fileName)); }

        private IDoxBook LoadBookFromFile(FileInfo f)
        {
            var name = f.Name.Substring(0, f.Name.Length - f.Extension.Length);
            String title;
            ParseBookFileName(name, out name, out title);
            var book = new InMemoryDoxBook()
            {
                Name = name,
                Title = title
            };
            using (var fs = f.Open(FileMode.Open))
            {
                var reader = new DoxZipFile.DoxZipFileReader(fs);
                reader.Process(book);
            }
            book.LastChangeTime = f.LastWriteTime;
            return book;
        }

        void ParseBookFileName(String name, out String bookName, out String bookTitle)
        {
            var slashIndex = name.IndexOf('-');
            if (slashIndex == -1)
            {
                bookName = name.Replace('.', '_');
                bookTitle = name;
            }
            else
            {
                bookName = name.Substring(0, slashIndex).Replace('.', '_');
                bookTitle = name.Substring(slashIndex + 1);
            }
        }

        string GetBookNameFromFileName(String name)
        {
            String bookName, bookTitle;
            ParseBookFileName(name, out bookName, out bookTitle);
            return bookName;
        }

        #endregion Library operations

        public new void Dispose()
        {
            if (bookFilesWatcher != null)
            {
                bookFilesWatcher.Created -= OnDoxFileChanged;
                bookFilesWatcher.Changed -= OnDoxFileChanged;
                bookFilesWatcher.Renamed -= OnDoxFileChanged;
                bookFilesWatcher.Deleted -= OnDoxFileChanged;
                bookFilesWatcher.Dispose();
            }

            base.Dispose();
        }
    }
}