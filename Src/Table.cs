using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace Csml {
    public class TableHeader { }
    public class TableHeader<T>: TableHeader where T: TableHeader<T> {
        protected T[] Sub;
        public int Num => Math.Max(1, Sub.Sum(x => x.Num));
        public int Depth => (Sub.Length==0)?1 : 1+Sub.Max(x => x.Depth);
    }

    public class Row : TableHeader<Row> {        
        public Row(string name, params Row[] subRows) {
            Sub = subRows;
        }
        
    }

    public class Column : TableHeader<Column> {
        public Column(string name, params Column[] subColumns) {
            Sub = subColumns;
        }
    }


    public class Table : Collection<Table> {
        TableHeader[] Headers;
        public int UserDefinedNumColumns;
        public Table(params TableHeader[] headers) {
            Headers = headers;
        }
        public Table(int numColumns) {
            UserDefinedNumColumns = numColumns;
        }

        private HtmlNode GenerateTableNoHeaders(Context context) {
            return HtmlNode.CreateNode("<table>");
        }

        private HtmlNode GenerateTable(Context context) {
            if (Headers == null) return GenerateTableNoHeaders(context);

            var rows = Headers.OfType<Row>();
            var columns = Headers.OfType<Column>();

            var numRows = rows.Sum(x => x.Num);
            var numColumns = columns.Sum(x => x.Num);

            var depthRows = (numRows > 0) ? rows.Max(x => x.Depth) : 0;
            var depthColumns = (numColumns > 0) ? columns.Max(x => x.Depth) : 0;
            var numElements = Elements.Count();

            if ((numColumns > 0) & (numRows == 0)) {
                numRows = numElements/numColumns + (((numElements%numColumns)>0)?1:0);
            }

           // List<KeyValuePair<int,int>> currentHeaderRow

            return HtmlNode.CreateNode("<table>").Do(x => {
                if (depthColumns > 0) {
                    x.Add("<thead>").Do(x => {
                        for (int i = 0; i < depthColumns; i++) {
                            x.Add("<tr>").Do(x=> {
                                if (depthRows > 0) {
                                    x.Add("<td>").SetAttributeValue("colspan", depthRows.ToString());
                                }
                                for (int c = 0; c < numColumns; c++) { 
                                
                                }
                            });
                        }
                    });
                }

            });

            //return GenerateTableNoHeaders(context);
        }


        public override IEnumerable<HtmlNode> Generate(Context context) {
            yield return GenerateTable(context); 
        }
    }


}