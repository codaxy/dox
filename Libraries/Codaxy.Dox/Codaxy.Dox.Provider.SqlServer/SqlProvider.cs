using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlProvider;
using Codaxy.Common.Logging;

namespace Codaxy.Dox.Providers
{
    public class SqlProvider : IDoxProvider 
    {

        private string connectionString;
        private string databaseName;

        public Logger Logger { get; set; }

        public SqlProvider(string connectionString, string databaseName) 
        {
            this.connectionString = connectionString;
            this.databaseName = databaseName;
        }

        #region IDoxProvider Members
        public void Process(IDoxBuilder builder)
        {
            foreach (Codaxy.Dox.DoxDocument doc in SqlSchemaReader.ReadSchema(connectionString,databaseName, Logger))
            {
                doc.GenerateTypeSearchItems();
                builder.Add(doc);
            }
        }
        #endregion
    }
}
