using System;
using System.Collections.Generic;

namespace Codaxy.Dox.Engine
{
    public class DoxBookNotFoundException : Exception { }

    public class DoxLibrary
    {
        Dictionary<String, IDoxBook> books;

        public DoxLibrary()
        {
            books = new Dictionary<String, IDoxBook>();
        }

        public IEnumerable<IDoxBook> GetBooks()
        {
            return books.Values;
        }

        public void AddBook(IDoxBook b)
        {
            books[b.Name] = b;
        }

        public IDoxBook GetBook(string name)
        {
            IDoxBook res;
            if (name != null && books.TryGetValue(name, out res))
                return res;
            throw new DoxBookNotFoundException();
        }

        public void Clear() { books.Clear(); }

        public void RemoveBook(string name)
        {
            books.Remove(name);
        }
    }
}