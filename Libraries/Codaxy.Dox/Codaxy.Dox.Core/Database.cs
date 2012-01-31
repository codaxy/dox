using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codaxy.Dox
{
    public class DbElement
    {
        public String DbName { get; set; }
        public Database Database { get; set; }
        public Table Table { get; set; }
        public View View { get; set; }
        public StoredProcedure StoredProcedure { get; set; }
    }

    public class Database
    {
        public string Name { get; set; }
        public String Description { get; set; }
        public List<Table> Tables { get; set; }
        public List<View> Views { get; set; }
        public List<StoredProcedure> StoredProcedures { get; set; }
    }

    public class Table 
    {
        public string Name          { get; set; }
        public String Description { get; set; }
        public List<Column> Columns { get; set; }
        public string SchemaName    { get; set; } 
    }

    public class View 
    {
        public string Name { get; set; }
        public String Description { get; set; }
        public List<Column> Columns { get; set; }
        public string SchemaName { get; set; }
    }

    public class StoredProcedure
    {
        public string Name { get; set; }
        public String Description { get; set; }
        public List<StoredProcedureParameter> Parameters { get; set; }
    }

    public class ReferenceData
    {
        public string ReferencedTable { get; set; }
        public string ReferencedTableSchema { get; set; }
        public string ReferencedColumn { get; set; }
    }

    public class Column 
    {
        public string Name          { get; set; }
        public string Descritpion   { get; set; }
        public DataType DataType    { get; set; }
        public bool Nullable        { get; set; }
        public bool IsInPrimaryKey  { get; set; }
        public bool IsForeignKey  { get; set; }
        public ReferenceData ReferenceData { get; set; }
    }

    public class DataType 
    {
        public string Name          { get; set; }
        public int? NumericPrecision { get; set; }
        public int? NumericScale     { get; set; }
        public int? MinimumSize      { get; set; }
        public int? MaximumLength      { get; set; } 
    }

    public class StoredProcedureParameter
    {
        public String Name { get; set; }
        public DataType DataType { get; set; }
        public bool IsOutputParameter { get; set; }
    }
}
