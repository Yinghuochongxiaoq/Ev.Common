using System;
using System.Data;
using System.Data.OleDb;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Ev.Common.ExcelHelper
{
    /// <summary>
    /// excel helper
    /// </summary>
    public class ExcelHelper
    {
        private readonly string _f;
        private IWorkbook _w;
        private FileStream _fs;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="filePath"></param>
        public ExcelHelper(string filePath)
        {
            _f = filePath;
        }

        /// <summary>
        /// 读取Excel表中的数据到DataSet中
        /// </summary>
        /// <returns>数据集合DataSet</returns>
        [Obsolete]
        public DataSet ReadExcelToDataSet()
        {
            if (string.IsNullOrEmpty(_f)) return null;
            string tblName = _f.Substring(_f.LastIndexOf("\\", StringComparison.Ordinal) + 1);
            string connStr;
            string fileType = Path.GetExtension(tblName);
            if (string.IsNullOrEmpty(fileType)) return null;

            if (fileType == ".xls") connStr = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + _f + ";" + ";Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1\"";
            else connStr = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + _f + ";" + ";Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1\"";
            const string sqlF = "Select * FROM [{0}]";

            OleDbConnection conn = null;
            OleDbDataAdapter da = null;

            DataSet ds = new DataSet();
            try
            {
                conn = new OleDbConnection(connStr);
                conn.Open();

                var dtSheetName = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables,
                    new object[] { null, null, null, "TABLE" });
                if (dtSheetName == null) return null;
                da = new OleDbDataAdapter();
                for (int i = 0; i < dtSheetName.Rows.Count; i++)
                {
                    var sheetName = (string)dtSheetName.Rows[i]["TABLE_NAME"];
                    if (sheetName.Contains("$") && !sheetName.Replace("'", "").EndsWith("$"))
                    {
                        continue;
                    }
                    da.SelectCommand = new OleDbCommand(string.Format(sqlF, sheetName), conn);
                    DataSet dsItem = new DataSet();
                    da.Fill(dsItem, sheetName);
                    ds.Tables.Add(dsItem.Tables[0].Copy());
                }
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                    da?.Dispose();
                    conn.Dispose();
                }
            }
            return ds;
        }

        /// <summary>
        /// 将DataTable数据导入到excel中
        /// </summary>
        /// <returns>导入数据行数(包含列名那一行)</returns>
        public int DataTableToExcel(DataTable data, string sheetName, bool isColumnWritten)
        {
            if (string.IsNullOrEmpty(_f)) return -1;
            _fs = new FileStream(_f, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            var index = _f.LastIndexOf('.');
            var extensionName = _f.Substring(index);
            if (string.Equals(extensionName, ".xlsx", StringComparison.CurrentCultureIgnoreCase))
            {
                _w = new XSSFWorkbook();
            }
            else if (string.Equals(extensionName, ".xls", StringComparison.CurrentCultureIgnoreCase))
            {
                _w = new HSSFWorkbook();
            }
            var cellStyle = _w.CreateCellStyle();
            cellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("@");

            try
            {
                ISheet sheet;
                if (_w != null)
                {
                    sheet = _w.CreateSheet(sheetName);
                }
                else
                {
                    return -1;
                }

                int count;
                int j;
                if (isColumnWritten)
                {
                    IRow row = sheet.CreateRow(0);
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        row.CreateCell(j).SetCellValue(data.Columns[j].ColumnName);
                    }
                    count = 1;
                }
                else
                {
                    count = 0;
                }

                int i;
                for (i = 0; i < data.Rows.Count; ++i)
                {
                    IRow row = sheet.CreateRow(count);
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        row.CreateCell(j).SetCellValue(data.Rows[i][j].ToString());
                        row.GetCell(j).CellStyle = cellStyle;
                    }
                    ++count;
                }
                _w.Write(_fs);
                return count;
            }
            finally
            {
                _fs.Close();
            }
        }

        /// <summary>
        /// 将excel中的数据导入到DataTable中
        /// </summary>
        /// <returns>返回的DataTable</returns>
        public DataTable ExcelToDataTable(string sheetName, bool isFirstRowColumn)
        {
            DataTable b = new DataTable();
            if (string.IsNullOrEmpty(_f)) return b;
            try
            {
                _fs = new FileStream(_f, FileMode.Open, FileAccess.Read);
                var index = _f.LastIndexOf('.');
                var extensionName = _f.Substring(index);
                if (string.Equals(extensionName, ".xlsx", StringComparison.CurrentCultureIgnoreCase))
                {
                    _w = new XSSFWorkbook(_fs);
                }
                else if (string.Equals(extensionName, ".xls", StringComparison.CurrentCultureIgnoreCase))
                {
                    _w = new HSSFWorkbook(_fs);
                }

                ISheet s;
                if (sheetName != null)
                {
                    s = _w.GetSheet(sheetName) ?? _w.GetSheetAt(0);
                }
                else
                {
                    s = _w.GetSheetAt(0);
                }
                if (s != null)
                {
                    IRow a = s.GetRow(0);
                    int v = a.LastCellNum;

                    int w;
                    if (isFirstRowColumn)
                    {
                        for (int i = a.FirstCellNum; i < v; ++i)
                        {
                            ICell cell = a.GetCell(i);
                            string cellValue = cell?.StringCellValue;
                            if (cellValue != null)
                            {
                                DataColumn column = new DataColumn(cellValue);
                                b.Columns.Add(column);
                            }
                        }
                        w = s.FirstRowNum + 1;
                    }
                    else
                    {
                        w = s.FirstRowNum;
                    }
                    int rowCount = s.LastRowNum;
                    for (int i = w; i <= rowCount; ++i)
                    {
                        IRow row = s.GetRow(i);
                        if (row == null) continue;

                        DataRow dataRow = b.NewRow();
                        for (int j = row.FirstCellNum; j < v; ++j)
                        {
                            if (row.GetCell(j) != null)
                                dataRow[j] = row.GetCell(j);
                        }
                        b.Rows.Add(dataRow);
                    }
                }
                return b;
            }
            finally
            {
                _fs.Close();
            }
        }

        /// <summary>
        /// excel导入DataSet中
        /// </summary>
        public DataSet ExcelToDataSet(bool isFirstRowColumn)
        {
            var z = new DataSet();
            if (string.IsNullOrEmpty(_f)) return z;
            try
            {
                _fs = new FileStream(_f, FileMode.Open, FileAccess.Read);
                if (_f.EndsWith(".xlsx")) _w = new XSSFWorkbook(_fs);
                else if (_f.EndsWith(".xls")) _w = new HSSFWorkbook(_fs);
                else return z;
                var a = _w.NumberOfSheets;
                if (a <= 0) return z;
                for (var sheetItem = 0; sheetItem < a; sheetItem++)
                {
                    var b = new DataTable();
                    var c = _w.GetSheetAt(sheetItem);
                    if (c == null) continue;
                    b.TableName = c.SheetName;
                    IRow d = c.GetRow(0);
                    int e = d.LastCellNum;
                    int f;
                    if (isFirstRowColumn)
                    {
                        for (int i = d.FirstCellNum; i < e; ++i)
                        {
                            var column = new DataColumn(d.GetCell(i).StringCellValue);
                            b.Columns.Add(column);
                        }
                        f = c.FirstRowNum + 1;
                    }
                    else
                    {
                        for (int i = d.FirstCellNum; i < e; ++i)
                        {
                            var column = new DataColumn();
                            b.Columns.Add(column);
                        }
                        f = c.FirstRowNum;
                    }
                    var k = c.LastRowNum;
                    for (var i = f; i <= k; ++i)
                    {
                        var v = c.GetRow(i);
                        if (v == null) continue;

                        var dataRow = b.NewRow();
                        for (int j = v.FirstCellNum; j < e; ++j)
                        {
                            if (v.GetCell(j) == null) continue;
                            var str = v.GetCell(j).ToString();
                            dataRow[j] = str;
                        }
                        b.Rows.Add(dataRow);
                    }
                    z.Tables.Add(b);
                }

                return z;
            }
            catch (Exception)
            {
                return z;
            }
        }
    }
}
