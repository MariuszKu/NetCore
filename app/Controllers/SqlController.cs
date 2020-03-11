using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;


namespace NETcore.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class SqlController : ControllerBase
    {

        public List<Table> tables = new List<Table>();

        [HttpGet]
        public IEnumerable<Table> Get()
        {
            tables.Add(new Table { Name="t1" });
            List<ColumnTable> ct = new List<ColumnTable>();
            ct.Add(new ColumnTable{ Name="id"} );

            tables.Add(new Table { Name="t2", ColumnsList =ct} );
            return tables;
        }


        [HttpGet]
        public IEnumerable<Table> Parse(string sql)
        {
            SqlHelperWorker sh = new SqlHelperWorker();
            sh.ParseBatch(sql);

            return sh.GetSelectTable;
        }
        
        [HttpPost]
        public string GenCommand([FromBody]TwoTable tabs){
            Console.WriteLine(tabs.Type);

            GenerateCommand gc = new GenerateCommand(tabs.source, tabs.dest);
            if(tabs.Type == "ins")
                return gc.CreateCommand(SqlCommand.Insert);
            else if(tabs.Type == "merg")
                return gc.CreateCommand(SqlCommand.MergeInsert);
            else if(tabs.Type == "update")
                return gc.CreateCommand(SqlCommand.Update);
            else if(tabs.Type == "select")
                return gc.CreateCommand(SqlCommand.Select);  
            else
                return "something went wrong";


        }


        public string test()
        {
            return "testx";

        }



    }
}
