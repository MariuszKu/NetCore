import { Component } from '@angular/core';
import { Table, ColumnTable, Tableex, TwoTables, sql} from './table'
import { DataService } from './data.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
 
    tableList: Tableex[] = [];     
    sqlcode: string="select a, b, c from d";   
    
    source: Table = {name: "test", alias:"", sqlDefinition:"", columnsList: null};
    dest: Table= {name: "test", alias:"", sqlDefinition:"", columnsList: null};
    
    
    constructor(private _table :DataService ){
      
      _table.getTables().subscribe( (data: Table[] ) => {
          for(let i of data){
            this.tableList.push(new Tableex(i));

          }

      });

    }

    mapNames(){
      for(let s of this.source.columnsList){
          this.dest.columnsList.forEach(element => {
            if(element.name == s.name){
              s.mapping = element.name;
            }
          });
  
      }
    }
  
  
    mapOrder(){
      let arr: string[]=[];
  
      for(let s of this.source.columnsList){
          for(let element of this.dest.columnsList) {
            if(!arr.includes(element.name)){
              s.mapping = element.name;
              arr.push(element.name);
              break;
            }
          };
      }
  
    }


    send(type: string){
      let tabs: TwoTables= {} as TwoTables;
      tabs.source = this.source;
      tabs.dest = this.dest;
      tabs.type = type;

      this._table.sendCommand(tabs).subscribe( (data:sql) => {
        this.sqlcode = data.query;
        console.log(data.query);

      }
        , err => {
          console.log(err);
        }
        );
    
    }
    
    add(t: Table){
      let tt = (t as Tableex);
      tt.ex =true;
      
      if(this.source != null && this.dest != null){
        this.source = null;
        this.dest = null;
      }

      if(this.source == null)
        this.source = t;
      else
        this.dest = t;
      
        
      
    }

    parse(){
      this.tableList = [];
      this._table.getParse(this.sqlcode).subscribe( (data: Table[] ) => {
        for(let i of data){
          this.tableList.push(new Tableex(i));

        }

    });

    }


}
