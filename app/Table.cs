using System;
using System.Collections.Generic;

namespace NETcore{
 public class ColumnTable
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool Key { get; set; }

        public string Constrain { get; set; }
        public string Mapping { get; set; }
        public string TableAlias { get; set; }
    }


    public class Table
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public string SqlDefinition {get;set;}
        public List<ColumnTable> ColumnsList { get; set; }

    }

    public class TwoTable{
        public string Type { get; set; }
        public Table source { get; set; }
        public Table dest { get; set; }

    }

}