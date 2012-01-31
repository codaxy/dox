using System.Collections.Generic;

namespace Codaxy.Dox
{
    /// <summary>
    /// DoxDocument
    /// </summary>
    public class DoxDocument
    {
        public string FullName { get; set; }

        public Type Type { get; set; }

        public DbElement DbElement { get; set; }

        public string FormatCode { get; set; }

        public string Title { get; set; }

        public List<DoxSearchItem> SearchItems { get; set; }

        public void GenerateTypeSearchItems()
        {
            if (Type != null)
            {
                SearchItems = new List<DoxSearchItem>();
                if (Type.Methods != null)
                    foreach (var m in Type.Methods)
                        AddSearchItem(DoxSectionType.Method, Util.ExtractMethodName(m.Name), m.Description);

                if (Type.Properties != null)
                    foreach (var m in Type.Properties)
                        AddSearchItem(DoxSectionType.Property, m.Name, m.Description);

                if (Type.Fields != null)
                    foreach (var m in Type.Fields)
                        AddSearchItem(DoxSectionType.Field, m.Name, m.Description);

                if (Type.Events != null)
                    foreach (var m in Type.Events)
                        AddSearchItem(DoxSectionType.Field, m.Name, m.Description);
            }

            if (DbElement != null)
            {
                SearchItems = new List<DoxSearchItem>();
                if (DbElement.Database != null)
                    AddSearchItem(DoxSectionType.Database, DbElement.Database.Name, DbElement.Database.Description);

                if (DbElement.Table != null)
                {
                    AddSearchItem(DoxSectionType.Table, DbElement.Table.Name, DbElement.Table.Description);
                    foreach (var c in DbElement.Table.Columns)
                        AddSearchItem(DoxSectionType.Column, c.Name, c.Descritpion);
                }

                if (DbElement.View != null)
                {
                    AddSearchItem(DoxSectionType.View, DbElement.View.Name, DbElement.View.Description);
                    foreach (var c in DbElement.View.Columns)
                        AddSearchItem(DoxSectionType.Column, c.Name, c.Descritpion);
                }

                if (DbElement.StoredProcedure != null)
                {
                    AddSearchItem(DoxSectionType.StoredProcedure, DbElement.StoredProcedure.Name, DbElement.StoredProcedure.Description);
                }
            }
        }

        private void AddSearchItem(DoxSectionType type, string name, string desc)
        {
            SearchItems.Add(new DoxSearchItem { SectionType = type, SectionName = name, SectionDescription = desc });
        }
    }

    public enum DoxSectionType
    {
        None, Text, Class, Field, Property, Method, Database, Table, View, StoredProcedure, Column
    }

    public class DoxSearchItem
    {
        public string SectionName { get; set; }

        public string SectionDescription { get; set; }

        public DoxSectionType SectionType { get; set; }
    }
}