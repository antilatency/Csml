using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace Csml {
    public enum TextAlignment {
        Left,
        Right,
        Center
    }

    public class TableHeader {
        public string Name;
        public TextAlignment Alignment = TextAlignment.Center;
        public class Cell {
            public object Element;
            public int Width;
            public int Height;

            public Cell(object element, int width, int height) {
                Element = element;
                Width = width;
                Height = height;
            }
        }
        public override string ToString() {
            return Name;
        }
    }
    public class TableHeader<T>: TableHeader where T: TableHeader<T> {
        
        protected T[] Sub;
        public int Num => Math.Max(1, Sub.Sum(x => x.Num));
        public int Depth => (Sub.Length==0)?1 : 1+Sub.Max(x => x.Depth);
        public IEnumerable<T> Flatten => (Sub.Length == 0) ? (new T[] { (T)this }) : Sub.SelectMany(x => x.Flatten);
        public TableHeader(string name, params T[] sub) {
            Name = name;
            Sub = sub;
        }

        public virtual void AddToMatrix(Cell[,] matrix, int x, int y, int width, int height) {
            matrix[x, y] = new Cell(this, width, height);
        }
    }

    public class Row : TableHeader<Row> {  
        
        public Row(string name, TextAlignment alignment, params Row[] sub) : base(name, sub) {
            Alignment = alignment;
        }
        public Row(string name, params Row[] sub) : base(name, sub) {
            Alignment = TextAlignment.Right;
        }

        public override void AddToMatrix(Cell[,] matrix, int x, int y, int width, int height) {
            base.AddToMatrix(matrix, x, y, width, height);
            var yPosition = y;
            foreach (var s in Sub) {
                var n = s.Num;
                s.AddToMatrix(matrix, x + 1, yPosition, 1, n);
                yPosition += n;
            }
        }
    }

    

    public class Column : TableHeader<Column> {        
        
        public Column(string name, TextAlignment alignment, params Column[] subColumns) : base(name, subColumns) {
            Alignment = alignment;
        }
        public Column(string name, params Column[] subColumns) : base(name, subColumns) {
            Alignment = TextAlignment.Center;
        }

        public override void AddToMatrix(Cell[,] matrix, int x, int y, int width, int height) {
            base.AddToMatrix(matrix, x, y, width, height);
            var xPosition = x;
            foreach (var s in Sub) {
                var n = s.Num;
                s.AddToMatrix(matrix, xPosition, y + 1, n, 1);
                xPosition += n;
            }
        }
    }


    public class Table : Collection<Table> {
        TableHeader[] Headers = new TableHeader[0];
        public int UserDefinedNumColumns;
        public float RowHeaderWidth = 0.1f;

        public Table(params string[] headers) {
            Headers = headers.Select(x=>new Column(x)).ToArray();
        }
        public Table(params TableHeader[] headers) {
            Headers = headers;
        }
        public Table(int numColumns) {
            UserDefinedNumColumns = numColumns;
        }

        private HtmlNode GenerateTableNoHeaders(Context context) {
            return HtmlNode.CreateNode("<table>");
        }

        public Table SetRowHeaderWidth(float width) {
            RowHeaderWidth = width;
            return this;
        }


        private HtmlNode GenerateTable(Context context) {
            //if (Headers == null) return GenerateTableNoHeaders(context);

            var rows = Headers.OfType<Row>();
            var columns = Headers.OfType<Column>();
            var alignments = columns.SelectMany(x => x.Flatten).Select(x => x.Alignment).ToArray();

            var numRows = rows.Sum(x => x.Num);
            var numColumns = columns.Sum(x => x.Num);

            var depthRows = (numRows > 0) ? rows.Max(x => x.Depth) : 0;
            var depthColumns = (numColumns > 0) ? columns.Max(x => x.Depth) : 0;
            var numElements = Elements.Count();

            

            if ((numColumns == 0) & (numRows > 0)) {
                numColumns = numElements / numRows + (((numElements % numRows) > 0) ? 1 : 0);
            }

            if ((numColumns == 0) & (numRows == 0)) {
                numColumns = Math.Max(1,UserDefinedNumColumns);
                alignments = Enumerable.Repeat(TextAlignment.Center, numColumns).ToArray();
            }

            if ((numColumns > 0) & (numRows == 0)) {
                numRows = numElements / numColumns + (((numElements % numColumns) > 0) ? 1 : 0);
            }

            TableHeader.Cell[,] matrix = new TableHeader.Cell[depthRows + numColumns, depthColumns + numRows];
            var xPosition = depthRows;
            foreach (var c in columns) {
                var depth = c.Depth;
                var num = c.Num;
                c.AddToMatrix(matrix, xPosition, 0, num, depthColumns - depth + 1);
                xPosition += num;
            }
            var yPosition = depthColumns;
            foreach (var r in rows) {
                var depth = r.Depth;
                var num = r.Num;
                r.AddToMatrix(matrix, 0, yPosition, depthRows - depth + 1, num);
                yPosition += num;
            }

            var elements = Elements.ToArray();
            for (int i = 0; i < elements.Length; i++) {
                var x = i % numColumns + depthRows;
                var y = i / numColumns + depthColumns;
                matrix[x, y] = new TableHeader.Cell(elements[i], 1, 1);
            }

            

            return HtmlNode.CreateNode("<table>").Do(x => {
                x.Add("<colgroup>").Do(x=> {
                    float normalizer = numColumns;
                    if (depthRows > 0) {
                        normalizer += RowHeaderWidth;
                        x.Add("<col>").Do(x=> {
                            x.SetAttributeValue("span", depthRows.ToString());
                            x.SetAttributeValue("style", $"width: {100*RowHeaderWidth/normalizer}%;");
                        });                            
                    }
                    for (int i = 0; i < numColumns; i++) {
                        x.Add("<col>").Do(x => {
                            x.SetAttributeValue("span", 1.ToString());
                            x.SetAttributeValue("style", $"width: {100 * 1 / normalizer}%;");
                        });
                    }

                });

                x.Add("<tbody>").Do(x => {
                    for (int r = 0; r < matrix.GetLength(1); r++) {
                            
                        x.Add("<tr>").Do(x=> {

                            if ((r == 0) & ((depthRows * depthColumns) > 0)) {
                                x.Add("<td>").Do(x => {
                                    x.SetAttributeValue("rowspan", depthColumns.ToString());
                                    x.SetAttributeValue("colspan", depthRows.ToString());
                                });
                            }

                            for (int c = 0; c < matrix.GetLength(0); c++) {
                                var cell = matrix[c, r];
                                if (cell != null) {
                                    HtmlNode node;
                                    if (cell.Element is TableHeader) {
                                        node = x.Add("<th>");
                                        //style="text-align:right"
                                        node.SetAttributeValue("style", $"text-align:{(cell.Element as TableHeader).Alignment.ToString().ToLower()}");
                                        node.InnerHtml = HtmlDocument.HtmlEncode((cell.Element as TableHeader).Name);
                                    } else {
                                        node = x.Add("<td>");
                                        node.SetAttributeValue("style", $"text-align:{alignments[c - depthRows].ToString().ToLower()}");
                                        node.Add((cell.Element as IElement).Generate(context));
                                    }
                                    node.Do(x => {
                                        if (cell.Width != 1) x.SetAttributeValue("colspan", cell.Width.ToString());
                                        if (cell.Height != 1) x.SetAttributeValue("rowspan", cell.Height.ToString());
                                    });
                                }
                            }
                        });
                    }
                });
                

            });

            //return GenerateTableNoHeaders(context);
        }


        public override IEnumerable<HtmlNode> Generate(Context context) {
            yield return GenerateTable(context); 
        }
    }


}