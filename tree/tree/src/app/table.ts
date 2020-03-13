export interface  ColumnTable
    {
        name;
        dataType;
        key;
        constrain;
        mapping;
        tableAlias;
    }

export interface sql{
    query: string;
}


    export class Table
    {
        name;
        alias;
        sqlDefinition ;
        columnsList: ColumnTable[];

    }

    export interface TwoTables{
        source: Table;
        dest: Table;
        type: string;

    }

    export class Tableex extends Table
    {
        name;
        alias;
        sqlDefinition ;
        columnsList: ColumnTable[];
        ex: boolean;
        

        constructor(table:Table){
            super();
            this.name = table.name;
            this.alias = table.alias;
            this.columnsList = table.columnsList;
            this.ex = false;
        }

    }


