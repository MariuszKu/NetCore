import { Component, OnInit, Input } from '@angular/core';
import {Table, ColumnTable, Tableex}  from '../table'

@Component({
  selector: 'app-grids',
  templateUrl: './grids.component.html',
  styleUrls: ['./grids.component.css']
})
export class GridsComponent implements OnInit {

  @Input("source") source: Table;
  @Input("dest") dest: Table;

  constructor() { 
    //this.source = {} as Table;

  }

 


  ngOnInit(): void {
  }

}
