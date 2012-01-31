using System;
using System.Collections.Generic;
using System.Text;

namespace Codaxy.Dox.Engine
{
    public class InMemoryDoxBook : IDoxBook, IDoxBuilder, IDoxProvider
    {
        class Chapter
        {
            public Chapter()
            {
                SubChapters = new List<Item>();
                Documents = new List<Item>();
            }

            public List<Item> SubChapters { get; private set; }

            public List<Item> Documents { get; private set; }
        }

        Dictionary<String, DoxDocument> data;

        Dictionary<String, Chapter> chapters;

        Dictionary<String, List<DoxSearchResult>> search;
        List<String> searchKeys;

        public InMemoryDoxBook()
        {
            data = new Dictionary<string, DoxDocument>();
            chapters = new Dictionary<string, Chapter>();
            search = new Dictionary<string, List<DoxSearchResult>>();
            chapters.Add("", new Chapter());
            LastChangeTime = DateTime.Now;
        }

        public DoxDocument GetDoxDocument(string docUrl)
        {
            DoxDocument dox;
            if (data.TryGetValue(docUrl, out dox))
                return dox;
            throw new DoxDocumentNotFoundException();
        }

        public List<DoxSearchResult> Search(string query)
        {
            List<DoxSearchResult> result = new List<DoxSearchResult>();
            if (string.IsNullOrEmpty(query))
                return new List<DoxSearchResult>();

            var q = query.ToLower();
            if (searchKeys == null)
            {
                searchKeys = new List<string>(search.Keys);
                searchKeys.Sort();
            }

            var pos = searchKeys.BinarySearch(q);
            if (pos < 0)
                pos = ~pos;

            while (pos < searchKeys.Count && searchKeys[pos].StartsWith(q))
            {
                result.AddRange(search[searchKeys[pos]]);
                pos++;
            }
            return result;
        }

        public string Name
        {
            get;
            set;
        }

        public string Title
        {
            get;
            set;
        }

        public DateTime LastChangeTime
        {
            get;
            set;
        }

        public void Add(DoxDocument dox)
        {
            data[dox.FullName] = dox;
            var parts = dox.FullName.Split('.'); //last part is doc name
            StringBuilder sb = new StringBuilder();
            for (var i = 0; i < parts.Length; i++)
            {
                var previousChapter = sb.ToString();
                var pc = AcquireChapter(previousChapter);
                if (i + 1 == parts.Length)
                    pc.Documents.Add(new Item { Url = dox.FullName, Title = dox.Title });
                else
                {
                    if (i > 0)
                        sb.Append(".");
                    sb.Append(parts[i]);
                    var chapter = sb.ToString();
                    if (!pc.SubChapters.Exists(a => a.Title == parts[i]))
                        pc.SubChapters.Add(new Item { Title = parts[i], Url = chapter });
                }
            }
            if (dox.SearchItems != null)
                foreach (var si in dox.SearchItems)
                {
                    GetSearchResultList(si.SectionName).Add(new DoxSearchResult
                    {
                        DocumentTitle = dox.Title,
                        SectionType = si.SectionType,
                        SectionDescription = si.SectionDescription,
                        SectionName = si.SectionName,
                        SectionTitle = si.SectionName,
                        DocumentFullName = dox.FullName
                    });
                }

            LastChangeTime = DateTime.Now;
        }

        private List<DoxSearchResult> GetSearchResultList(string keyword)
        {
            List<DoxSearchResult> res;
            var lower = keyword.ToLower();
            if (search.TryGetValue(lower, out res))
                return res;
            res = new List<DoxSearchResult>();
            search.Add(lower, res);
            searchKeys = null;
            return res;
        }

        private Chapter AcquireChapter(string chapter)
        {
            Chapter c;
            if (chapters.TryGetValue(chapter ?? "", out c))
                return c;
            c = new Chapter();
            chapters.Add(chapter, c);
            return c;
        }

        public List<Item> GetSubChaptersOf(string chapter)
        {
            Chapter c;
            if (chapters.TryGetValue(chapter ?? "", out c))
                return c.SubChapters;
            return new List<Item>();
        }

        public List<Item> GetChapterDocumentsOf(string chapter)
        {
            Chapter c;
            if (chapters.TryGetValue(chapter ?? "", out c))
                return c.Documents;
            return new List<Item>();
        }

        public void Process(IDoxBuilder builder)
        {
            foreach (var dox in data.Values)
                builder.Add(dox);
        }
    }
}