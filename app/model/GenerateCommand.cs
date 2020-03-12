using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;


namespace NETcore
{
    public enum SqlCommand
    {
        Select,
        Insert,
        Update,
        MergeInsert
    }


    public class GenerateCommand
    {
        private Table table;
        private Table descTable;

        public GenerateCommand(Table table, Table descTable) {
            this.table = table;
            this.descTable = descTable;
        }

      


        /*public string CreateStm()
        {
            RTTgenerator gen = new RTTgenerator();
            gen.table = this.table;
            gen.descTable = this.descTable;

            return gen.TransformText();

        }*/

        public string CreateCommand(SqlCommand sc)
        {
            var column = String.Join(", ", descTable.columnsList.Where(z => z.mapping != null).Select(z => z.name));
            var result = from d in descTable.columnsList
                         join s in table.columnsList on d.name equals s.mapping
                         select new { v1 = d.name, v2 = s.mapping, v3 = s.name, key=d.key };
            var r1 = result.ToList().Select(z => String.Format("{0}", z.v1)).ToArray();
            var r2 = result.ToList().Select(z => z.v2 != z.v3 && sc != SqlCommand.MergeInsert ? String.Format("{0} AS {1}", z.v3, z.v2) : String.Format("{0}", z.v2)).ToArray();
            var r3 = result.Where(z=> !z.key).ToList().Select(z => String.Format("{0} = sc.{1}", z.v1, z.v2)).ToArray();
            string descCols = String.Join(",", r1);
            string selectCols = String.Join("\n\t,", r2);
            string updateCols = String.Join("\n,", r3);

            switch (sc)
            {
                case SqlCommand.MergeInsert:
                    return String.Format("INSERT ({0}) \n VALUES ({1}) ", descCols, selectCols);
                case SqlCommand.Insert:    
                    if (!table.name.Contains("query"))
                       return String.Format("TRUNCATE TABLE {0};\nINSERT INTO {0} ({1}) \nSELECT\n\t {2} \nFROM\n {3}", descTable.name, descCols, selectCols, table.name);
                    else
                        return String.Format("TRUNCATE TABLE {0};\nINSERT INTO {0} ({1}) \nSELECT\n\t {2} \nFROM\n ({3}) A", descTable.name, descCols, selectCols, table.sqlDefinition);
                case SqlCommand.Select:
                    if (!table.name.Contains("query"))
                        return String.Format("SELECT\n {0} \nFROM {1}", selectCols, table.name);
                    else
                        return String.Format("SELECT {0} FROM ({1}) A", selectCols, table.sqlDefinition);
                case SqlCommand.Update:
                    return updateCols;
                default:
                    return "";

            }
        }

        public string UpdateStm(){

            var result = from d in descTable.columnsList.Where(z => z.key == true).ToList()
                         join s in table.columnsList on d.name equals s.mapping
                         select new { v1 = d.name, v2 = s.mapping };
            var r = result.ToList().Select(z => String.Format("dst.{1} = sc.{2}", descTable.name, z.v1, z.v2)).ToArray();
            string join = String.Join(" AND ", r);

            return String.Format("UPDATE dst SET\n {0} \nFROM {1} sc INNER JOIN {2} dst ON {3}",
            CreateCommand(SqlCommand.Update)
            ,this.table.name
            ,descTable.name
            ,join
            );
        }

        internal string SelectWithAliases()
        {
            string command = "SELECT \n";

            foreach(var item in table.columnsList)
            {
                command += item.name + ",\n";

            }

            command += "FROM " + table.sqlDefinition;

            return command;
        }

        internal string createMerge()
        {
            var srcColumn = String.Join(", ", table.columnsList.Where(z => z.mapping != null).Select(z => z.name));
            var descColumn = String.Join(", ", descTable.columnsList.Select(z => z.name));
            var column = String.Join(", ", descTable.columnsList.Select(z => z.name));
            var result = from d in descTable.columnsList.Where(z => z.key == true).ToList()
                         join s in table.columnsList on d.name equals s.mapping
                         select new { v1 = d.name, v2 = s.mapping };
            var r = result.ToList().Select(z => String.Format("{0}.{1} = sc.{2}", descTable.name, z.v1, z.v2)).ToArray();
            string join = String.Join(" AND ", r);
            return String.Format(@"MERGE INTO {0} USING(
{1}
) sc ON ({2})
WHEN MATCHED THEN UPDATE SET
{3} 
WHEN NOT MATCHED THEN 
{4}
", descTable.name, CreateCommand(SqlCommand.Select), join, CreateCommand(SqlCommand.Update), CreateCommand(SqlCommand.MergeInsert));

        }


  /*public void MappByname()
        {
            HlpTable hlp = null;
            foreach(var item in table.columnsList)
            {
                item.mapping = item.mapping == null || item.mapping == "" ? item.name : item.mapping;
                hlp = descTable.columnsList.Where(z => z.name.ToUpper() == item.mapping.ToUpper()).FirstOrDefault();
                if (hlp != null)
                    item.MapDestColumn = hlp.name;

            }

        }*/

        /*public void MappByOrader()
        {
            HlpTable hlp = null;
            int i = 0;
            
            foreach (var item in table.columnsList)
            {
                if (i < descTable.columnsList.Count)
                {
                    item.mapping = item.mapping == null || item.mapping == "" ? item.name : item.mapping;
                    hlp = descTable.columnsList[i];
                    if (hlp != null)
                        item.MapDestColumn = hlp.name;
                    i++;
                }
            }

        }*/

    }
}