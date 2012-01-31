using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Codaxy.Common.Logging;

namespace SqlProvider
{
    class SqlSchemaReader
    {
        public static List<Codaxy.Dox.DoxDocument> ReadSchema(string connectionString, String databaseName, Logger logger)
        {
            SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder(connectionString);
            var sc = new ServerConnection(csb.DataSource);
            if (!csb.IntegratedSecurity)
            {
                sc.LoginSecure = false;
                sc.Login = csb.UserID;
                sc.Password = csb.Password;
            }

            var server = new Server(sc);
            var database = server.Databases[databaseName];

            // Create Bock

            Codaxy.Dox.Database BaseElement = new Codaxy.Dox.Database();
            BaseElement.Name = database.Name;
            BaseElement.Tables = new List<Codaxy.Dox.Table>();
            BaseElement.Views = new List<Codaxy.Dox.View>();
            BaseElement.StoredProcedures = new List<Codaxy.Dox.StoredProcedure>();

            // Read Tables

            logger.Info("Read tables");
            foreach (Table table in database.Tables)
            {
                if (table.IsSystemObject) continue;
                Codaxy.Dox.Table _table = new Codaxy.Dox.Table();
                _table.Name = table.Name;
                _table.SchemaName = table.Schema;
                _table.Columns = new List<Codaxy.Dox.Column>();

                Dictionary<String, Codaxy.Dox.ReferenceData> references = new Dictionary<string, Codaxy.Dox.ReferenceData>();

                foreach (ForeignKey key in table.ForeignKeys)
                    foreach (ForeignKeyColumn column in key.Columns)
                    {
                        Codaxy.Dox.ReferenceData rd = new Codaxy.Dox.ReferenceData
                        {
                            ReferencedTable = key.ReferencedTable,
                            ReferencedTableSchema = key.ReferencedTableSchema,
                            ReferencedColumn = column.ReferencedColumn
                        };
                        references.Add(column.Name, rd);
                    }

                foreach (Column column in table.Columns)
                {
                    Codaxy.Dox.Column _column = new Codaxy.Dox.Column()
                    {
                        Name = column.Name,
                        @Nullable = column.Nullable,
                        IsForeignKey = column.IsForeignKey,
                        IsInPrimaryKey = column.InPrimaryKey,
                        Descritpion = (column.ExtendedProperties.Contains("MS_Description")) ? column.ExtendedProperties["MS_Description"].Value.ToString() : ""
                    };

                    Codaxy.Dox.ReferenceData rd;
                    if (column.IsForeignKey && references.TryGetValue(column.Name, out rd))
                        _column.ReferenceData = rd;

                    _column.DataType = ConvertDataType(column.DataType);
                    _table.Columns.Add(_column);
                }

                BaseElement.Tables.Add(_table);

            }

            // Read Views 
            logger.Info("Read views");
            foreach (View view in database.Views)
            {
                if (view.IsSystemObject) continue;
                Codaxy.Dox.View _view = new Codaxy.Dox.View()
                {
                    Name = view.Name,
                    SchemaName = view.Schema
                };

                foreach (Column column in view.Columns)
                {
                    Codaxy.Dox.Column _column = new Codaxy.Dox.Column()
                    {
                        Name = column.Name,
                        Descritpion = (column.ExtendedProperties.Contains("MS_Description")) ? column.ExtendedProperties["MS_Description"].Value.ToString() : ""
                    };

                    _column.DataType = ConvertDataType(column.DataType);
                    _view.Columns.Add(_column);
                }

                BaseElement.Views.Add(_view);
            }

            logger.Info("Read stored procedures");
            // StoreProcedure
            foreach (StoredProcedure sp in database.StoredProcedures)
            {
                if (sp.IsSystemObject) continue;
                Codaxy.Dox.StoredProcedure _sp = new Codaxy.Dox.StoredProcedure
                {
                    Name = sp.Name,
                    Parameters = new List<Codaxy.Dox.StoredProcedureParameter>()
                };

                foreach (StoredProcedureParameter _spp in sp.Parameters)
                {
                    _sp.Parameters.Add(new Codaxy.Dox.StoredProcedureParameter
                    {
                        Name = _spp.Name,
                        DataType = ConvertDataType(_spp.DataType),
                        IsOutputParameter = _spp.IsOutputParameter
                    });
                }

                BaseElement.StoredProcedures.Add(_sp);
            }

            logger.Info("Finished reading objects");

            List<Codaxy.Dox.DoxDocument> docList = new List<Codaxy.Dox.DoxDocument>();
            foreach (Codaxy.Dox.Table table in BaseElement.Tables)
                docList.Add(new Codaxy.Dox.DoxDocument()
                {
                    FullName = BaseElement.Name + ".Tables." + table.SchemaName + "." + table.Name,
                    Title = table.Name,
                    DbElement = new Codaxy.Dox.DbElement()
                    {
                        Table = table,
                        DbName = BaseElement.Name
                    }
                });
            foreach (Codaxy.Dox.View view in BaseElement.Views)
                docList.Add(new Codaxy.Dox.DoxDocument()
                {
                    FullName = BaseElement.Name + ".Views." + view.SchemaName + "." + view.Name,
                    Title = view.Name,
                    DbElement = new Codaxy.Dox.DbElement()
                    {
                        DbName = BaseElement.Name,
                        View = view
                    }
                });
           foreach (Codaxy.Dox.StoredProcedure sp in BaseElement.StoredProcedures)
               docList.Add(new Codaxy.Dox.DoxDocument
               {
                   FullName = BaseElement.Name + ".Procedures." + sp.Name,
                   Title = sp.Name,
                   DbElement = new Codaxy.Dox.DbElement
                   {
                       DbName = BaseElement.Name,
                       StoredProcedure = sp
                   }
               });


           docList.Add(new Codaxy.Dox.DoxDocument()
           {
               FullName = BaseElement.Name + "." + BaseElement.Name,
               Title = BaseElement.Name,
               DbElement = new Codaxy.Dox.DbElement()
               {
                   Database = BaseElement,
                   DbName = BaseElement.Name
               }
           });

            return docList;

        }

        private static Codaxy.Dox.DataType ConvertDataType(DataType type)
        {
            var t = new Codaxy.Dox.DataType
            {
                Name = type.Name
            };

            switch (type.SqlDataType)
            {
                case SqlDataType.Binary:
                case SqlDataType.Char:
                case SqlDataType.NChar:
                case SqlDataType.VarChar:
                case SqlDataType.NVarChar:
                case SqlDataType.VarBinaryMax:
                case SqlDataType.DateTime2:
                case SqlDataType.DateTimeOffset:
                case SqlDataType.Time:
                    t.MaximumLength = type.MaximumLength;
                    break;

                case SqlDataType.Decimal:
                case SqlDataType.Numeric:
                    t.NumericPrecision = type.NumericPrecision;
                    t.NumericScale = type.NumericScale;
                    break;

                default:
                    break;
            }

            return t;
        }

    }
}
