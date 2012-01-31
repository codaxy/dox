using System;
using System.Collections.Generic;

namespace Codaxy.Dox.Engine
{
    /// <summary>
    /// Search result
    /// </summary>
    public class DoxSearchResult
    {
        public String SectionName { get; set; }

        public String SectionTitle { get; set; }

        public String SectionDescription { get; set; }

        public String DocumentTitle { get; set; }

        public String DocumentFullName { get; set; }

        public DoxSectionType SectionType { get; set; }
    }

    public class DoxDocumentNotFoundException : Exception { }

    public class Item
    {
        public String Title { get; set; }

        public String Url { get; set; }
    }

    /// <summary>
    ///Class for retrieving documentation
    /// </summary>
    public interface IDoxBook
    {
        /// <summary>
        /// Gets the dox document.
        /// </summary>
        /// <param name="docUrl">The doc URL.</param>
        /// <returns></returns>
        Codaxy.Dox.DoxDocument GetDoxDocument(string docUrl);

        /// <summary>
        /// Searches the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        List<DoxSearchResult> Search(string query);

        List<Item> GetSubChaptersOf(String chapter);

        List<Item> GetChapterDocumentsOf(String chapter);

        /// <summary>
        /// Name of the book.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Title
        /// </summary>
        string Title { get; }

        DateTime LastChangeTime { get; }
    }
}