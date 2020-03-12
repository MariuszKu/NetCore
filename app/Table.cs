using System;
using System.Collections.Generic;

namespace NETcore{


    public class Sql{
            public string query { get; set; }

    }
 public class ColumnTable
    {
        public string name { get; set; }
        public string dataType { get; set; }
        public bool key { get; set; }

        public string constrain { get; set; }
        public string mapping { get; set; }
        public string tableAlias { get; set; }
    }


    public class Table
    {
        public string name { get; set; }
        public string alias { get; set; }
        public string sqlDefinition {get;set;}
        public List<ColumnTable> columnsList { get; set; }

    }

    public class TwoTable{
        public string type { get; set; }
        public Table source { get; set; }
        public Table dest { get; set; }

    }

}