using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;


namespace NETcore.Controllers
{
    //[ApiController]
    [Route("[controller]/[action]")]
    public class SqlController : ControllerBase
    {

        public List<Table> tables = new List<Table>();

        [HttpGet]
        public IEnumerable<Table> Get()
        {
            tables.Add(new Table { name="t1" });
            List<ColumnTable> ct = new List<ColumnTable>();
            ct.Add(new ColumnTable{ name="id"} );

            tables.Add(new Table { name="t2", columnsList =ct} );
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
        public Sql GenCommand([FromBody]TwoTable tabs){
            Console.WriteLine(tabs.type);
            Sql sql = new Sql();
            
            GenerateCommand gc = new GenerateCommand(tabs.source, tabs.dest);
            if(tabs.type == "ins"){
                sql.query = gc.CreateCommand(SqlCommand.Insert);
                return sql;
            }
            else if(tabs.type == "merge"){
                sql.query =gc.createMerge();
                return sql;
            }
            else if(tabs.type == "update"){
                sql.query =gc.UpdateStm();
                 return sql;
            }
            else if(tabs.type == "select"){
                sql.query =gc.CreateCommand(SqlCommand.Select);  
                 return sql;
            }
            else{
                sql.query = "something went wrong";
                 return sql;
            }
            
            
        }


        public string test()
        {
            return "testx";

        }



    }
}
