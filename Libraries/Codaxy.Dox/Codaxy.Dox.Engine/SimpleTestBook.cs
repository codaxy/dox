using System;
using System.Collections.Generic;
using Codaxy.Common.Text;

namespace Codaxy.Dox.Engine
{
    public class SimpleTestBook : Engine.IDoxBook
    {
        public DoxDocument GetDoxDocument(string docUrl)
        {
            return new DoxDocument() { FormatCode = "dummy" };
        }

        public List<DoxSearchResult> Search(string query)
        {
            throw new NotImplementedException();
        }

        public List<Engine.Item> GetSubChaptersOf(string chapter)
        {
            if (!chapter.IsNullOrEmpty())
                return new List<Engine.Item>();
            else
                return new List<Engine.Item>
                {
                    new Engine.Item {
                        Title = "Chapter 1",
                        Url = String.Format("{0}.ch0", chapter).Replace(Name, "").TrimStart('.')
                    },
                    new Engine.Item {
                        Title = "Chapter 2",
                        Url = String.Format("{0}.ch1", chapter).Replace(Name, "").TrimStart('.')
                    }
                };
        }

        public List<Engine.Item> GetChapterDocumentsOf(string chapter)
        {
            return new List<Engine.Item>
            {
                new Engine.Item {
                    Title = "Document 1",
                    Url = String.Format("{0}.doc0", chapter).Replace(Name, "").TrimStart('.')
                }
            };
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
    }
}