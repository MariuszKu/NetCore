import { Injectable } from '@angular/core';
import { HttpClient,  HttpParams, HttpHeaders } from '@angular/common/http';
import {Table, TwoTables, ColumnTable} from './table'
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DataService {

  constructor(private http: HttpClient) { }

  getParse(sql: string){
    return this.http.get<Table[]>("http://localhost:5000/Sql/Parse", {params: {"sql":sql}});

  }

  getTables(){

    return this.http.get<Table[]>("http://localhost:5000/Sql/Get");

  }

  private httpOptions = {
    headers: new HttpHeaders({
        'Accept': 'text/html',
        'Content-Type': 'application/json'
    })
    
  };

  sendCommand(tabs1: TwoTables){
         
    return this.http.post("http://localhost:5000/Sql/GenCommand", tabs1, this.httpOptions);
    
  }


}

