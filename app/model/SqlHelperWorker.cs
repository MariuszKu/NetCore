using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;


namespace NETcore
{
    class SqlHelperWorker
    {

        public List<Table> GetSelectTable { get; set; }
        private List<string> tablesWithDef { get; set; }

        public SqlHelperWorker()
        {
            GetSelectTable = new List<Table>();
        }

        public void ParseBatch(string query)
        {
            var qs = query.Split(';');
            int i = 0;
            foreach(var item in qs)
            {
                if (Regex.IsMatch(item.ToUpper(), @"SELECT([\s\S]*)FROM.*"))
                {
                    ParseSelect(item);
                }
                else
                {
                    ParseDDLCreate(item, i);
                    i++;
                }
            }

        }


        public string ParseDDLCreate(string q1, int xid=1)
        {
            string q = "";
            bool key=false;
            Table t = new Table();
            t.columnsList = new List<ColumnTable>();
            q = Regex.Match(q1, @"\(([\s\S]*)\)[^(]*$", RegexOptions.Multiline).Groups[1].Value;
            t.name = Regex.Match(q1, @"table\s+([a-z,0-9,\.,\[,\]]+).*\n?.*\(", RegexOptions.Multiline | RegexOptions.IgnoreCase).Groups[1].Value;
            var a = Regex.Split(q, @",(?![^\(]+\))");
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = a[i].Trim();
                if (a[i].Length>0)
                {
                    //var w = Regex.Split(a[i], @"\s(?![^\(]+\))(?![^\s][^\(]+\))",RegexOptions.IgnorePatternWhitespace);
                    var w = a[i].Split(new String[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    var constraint = ArrayHelper.Contraint(w);
                    if (w.Length > 2)
                        if (w[2].StartsWith("("))
                        {
                            w[1] += w[2];
                            w[2] = w[3];
                        }
                    w[0] = w[0].Replace("[", "").Replace("]", "");
                    key = constraint.ToUpper().Contains("PRIMARY key");
                    if(!new String[] { "constrainT" }.Contains(w[0]))
                        t.columnsList.Add(new ColumnTable { name = w[0].ToUpper(), dataType = w[1].ToUpper(), constrain = ArrayHelper.Contraint(w), key = key, tableAlias = t.name });
                    key = false;
                }
            }
            q = String.Join(",", a);
            this.GetSelectTable.Add(t);
            return q;
        }

        

        public void ParseSelect(string q2)
        {
            if (!Regex.IsMatch(q2.ToUpper(), @"SELECT([\s\S]*)FROM.*"))
            {
                ParseDDLCreate(q2, 0);

            }
            else
            {
                Table t = new Table();
                t.columnsList = new List<ColumnTable>();
                tablesWithDef = new List<string>();
                string REGEX_MATCH_TABLE_name = @"(?<=(?:FROM|JOIN)[\s(]+)(?>\w+)(?=[\s)]*(?:\s+(?:AS\s+)?\w+)?(?:$|\s+(?:WHERE|ON|(?:LEFT|RIGHT)?\s+(?:(?:OUTER|INNER)\s+)?JOIN)))";
                t.name = "query-" +Regex.Match(q2, REGEX_MATCH_TABLE_name, RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups[0].Value;

                foreach(var item in Regex.Match(q2, REGEX_MATCH_TABLE_name, RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups)
                {
                    if(GetSelectTable.Count>0)
                        if(GetSelectTable.Where(z => z.name == item.ToString()).FirstOrDefault()!= null)
                            tablesWithDef.Add(GetSelectTable.Where(z => z.name == item.ToString()).First().name);
                }

                t.sqlDefinition = q2;

                var a = ArrayHelper.GetColumns(q2);
                string[] arr = new string[a.Length];
                for (int i = 0; i < a.Length; i++)
                {
                    a[i] = a[i].Trim();
                    var calies = ArrayHelper.GetColumnAndAlias(a[i]);
                    if (calies[0].Split('.').Length == 1)
                        t.columnsList.Add(new ColumnTable { name = calies[0], mapping = calies[1], tableAlias = t.name });
                    else
                    {
                        string mapname = calies[1].Split('.').Length > 1 ? calies[1].Split('.')[1] : calies[1];
                        t.columnsList.Add(new ColumnTable { name = calies[0], mapping = mapname, tableAlias = ArrayHelper.findAlias(calies[0]) });
                    }
                }
                this.GetSelectTable.Add(t);
            }
        }

        private string ExtractDef(string q2)
        {

            int i = 0;
            int s = q2.ToUpper().IndexOf("FROM");
            string txt = q2.Substring(s, q2.Length-s);
            if (txt.Contains("openquery"))
            {
                i = txt.IndexOf("(");
                int bracket = 0;
                for (; i < txt.Length; i++)
                {
                    switch (txt[i])
                    {
                        case '(':
                            bracket++;
                            break;
                        case ')':
                            bracket--;
                            break;


                    }
                    if (bracket == 0)
                        break;
                }
            }
            return txt.Substring(4, i+1);
        }

    
        internal string getInsert(bool ignoreChar)
        {

            var column = String.Join(", ", GetSelectTable[1].columnsList.Where(z=> z.mapping != null).Select(z=> z.name  ));
            return String.Format("INSERT INTO {0} ({1})\n {2}", GetSelectTable[1].name, column, getmapping(ignoreChar));
        }

        internal string getUpsert(bool ignoreChar)
        {
            var srcColumn = String.Join(", ", GetSelectTable[1].columnsList.Where(z => z.mapping != null).Select(z => z.name));
            var descColumn = String.Join(", ", GetSelectTable[0].columnsList.Select(z => z.name));
            var column = String.Join(", ", GetSelectTable[1].columnsList.Select(z => z.name));
            var result = from d in GetSelectTable[1].columnsList.Where(z => z.key == true).ToList()
                         join s in GetSelectTable[0].columnsList on d.name equals s.name
                         select new { v1 = d.name, v2 = s.name };
            var r = result.ToList().Select(z => String.Format("{0}.{1} = sc.{2}", GetSelectTable[1].name, z.v1, z.v2)).ToArray();
            string join = String.Join(" AND ", r);
            return String.Format(@"MERGE INTO {0} USING(
{2}
) sc ON ({3})
WHEN MATCHED THEN UPDATE SET
{4} 
WHEN NOT MATCHED THEN INSERT
({5}) VALUES {6}
", GetSelectTable[1].name, column, getmapping(ignoreChar), join, getUpdatemapping(), srcColumn, getInsertmapping());
        }

        public string getInsertmapping()
        {
            var column = String.Join(", ", GetSelectTable[1].columnsList.Where(z=> z.mapping!=null).Select(z => z.name));
            var result = from d in GetSelectTable[1].columnsList
                         join s in GetSelectTable[0].columnsList on d.mapping equals s.mapping
                         select new { v1 = d.mapping, v2 = s.mapping };
            var r = result.ToList().Select(z => String.Format("sc.{0}", z.v2)).ToArray();
            string join = String.Join(",", r);
            return String.Format(@"({0})",join);

        }


        public string getUpdate(bool ignoreChar)
        {
            var column = String.Join(", ", GetSelectTable[1].columnsList.Where(z=> z.mapping != null).Select(z => z.name));
            var result = from d in GetSelectTable[1].columnsList.Where(z => z.key==true).ToList()
                         join s in GetSelectTable[0].columnsList on d.mapping equals s.mapping
                         select new { v1 = d.mapping, v2= s.mapping };
            var r = result.ToList().Select(z => String.Format("{0}.{1} = sc.{2}", GetSelectTable[1].name, z.v1, z.v2)).ToArray();
            string join = String.Join(" AND ", r);
            return String.Format(@"MERGE INTO {0} USING(
{2}
) sc ON ({3})
WHEN MATCHED THEN UPDATE 
SET {4} ", GetSelectTable[1].name, column, getmapping(ignoreChar), join, getUpdatemapping());

        }

/*
        internal void MapByname(ListView lvColumns)
        {
            foreach(var item in GetSelectTable[1].columnsList)
            {

                var c = GetSelectTable[0].columnsList.Where(z => z.mapping == item.name).FirstOrDefault();
                if (c != null)
                    item.mapping = c.mapping;

            }
            ICollectionView view = CollectionViewSource.GetDefaultView(GetSelectTable[1].columnsList);
            view.Refresh();

        }
*/
       

        internal string getmapping(bool? IgnoreChars)
        {
            bool ignoreChars = IgnoreChars ?? false;
            var arr = new List<string>();
            var i = 0;
            foreach(var row in this.GetSelectTable[1].columnsList)
            {
                var item = this.GetSelectTable[0].columnsList.Where(z => z.mapping == row.mapping).FirstOrDefault();
                if (item != null)
                {
                    if (!(ignoreChars && row.dataType.Contains("CHAR")))
                        arr.Add(String.Format("CAST({0} AS {1}) {2}\n", item.name, row.dataType, row.name));
                    else
                        arr.Add(String.Format("{0} AS  {1}\n", item.mapping, row.name));
                    i++;
                }
            }

            var query = String.Join(",", arr.ToArray());
            return String.Format("SELECT \n{0} FROM \n{1}", query, GetSelectTable[0].name);
        }

        internal string getUpdatemapping()
        {
            var arr = new List<string>();//new string[this.GetSelectTable[1].columnsList.Count];
            
            foreach (var row in this.GetSelectTable[1].columnsList)
            {
                if (!row.key && row.mapping != null)
                {
                    arr.Add(String.Format("{0} = sc.{1}\n", row.name, row.mapping));
                }
                
            }

            var query = String.Join(",", arr.ToArray());
            return String.Format("{0}", query);
        }
    }
}