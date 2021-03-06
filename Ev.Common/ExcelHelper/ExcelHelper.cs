﻿/**============================================================
* 命名空间: Ev.Common.Util
*
* 功 能： excel 帮助类
* 类 名： ExcelHelper
*
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
* V0.01 2017/5/19 14:26:12 FreshMan 初版
*
* Copyright (c) 2017 Lir Corporation. All rights reserved.
*==============================================================
*==此技术信息为本公司机密信息,未经本公司书面同意禁止向第三方披露==
*==版权所有：重庆慧都科技有限公司                             ==
*==============================================================
*/

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
        #region [0、私有变量]

        /// <summary>
        /// 文件路径+文件名
        /// </summary>
        private readonly string _fileName;

        /// <summary>
        /// 工作簿
        /// </summary>
        private IWorkbook _workbook;

        /// <summary>
        /// 文件读写流
        /// </summary>
        private FileStream _fs;
        #endregion

        #region [1、构造函数]

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="filePath"></param>
        public ExcelHelper(string filePath)
        {
            _fileName = filePath;
        }
        #endregion

        #region [2、使用微软的Microsoft.ACE.LOEDB读取Excel]
        /// <summary>
        /// 读取Excel表中的数据到DataSet中
        /// </summary>
        /// <returns>数据集合DataSet</returns>
        [Obsolete]
        public DataSet ReadExcelToDataSet()
        {
            if (string.IsNullOrEmpty(_fileName)) return null;
            string tblName = _fileName.Substring(_fileName.LastIndexOf("\\", StringComparison.Ordinal) + 1);
            string connStr;
            //获取文件扩展名称
            string fileType = Path.GetExtension(tblName);
            if (string.IsNullOrEmpty(fileType)) return null;

            if (fileType == ".xls") connStr = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + _fileName + ";" + ";Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1\"";//HDR=YES;第一列作为标题 IMEX=0不读为字符串
            else connStr = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + _fileName + ";" + ";Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1\"";
            const string sqlF = "Select * FROM [{0}]";

            OleDbConnection conn = null;
            OleDbDataAdapter da = null;

            DataSet ds = new DataSet();
            try
            {
                // 初始化连接，并打开
                conn = new OleDbConnection(connStr);
                conn.Open();

                var dtSheetName = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables,
                    new object[] { null, null, null, "TABLE" });
                if (dtSheetName == null) return null;
                // 初始化适配器
                da = new OleDbDataAdapter();
                for (int i = 0; i < dtSheetName.Rows.Count; i++)
                {
                    // 获取数据源的表定义元数据
                    var sheetName = (string)dtSheetName.Rows[i]["TABLE_NAME"];
                    if (sheetName.Contains("$") && !sheetName.Replace("'", "").EndsWith("$"))
                    {
                        continue;
                    }
                    da.SelectCommand = new OleDbCommand(String.Format(sqlF, sheetName), conn);
                    DataSet dsItem = new DataSet();
                    da.Fill(dsItem, sheetName);
                    ds.Tables.Add(dsItem.Tables[0].Copy());
                }
            }
            catch (Exception ex)
            {
                // ReSharper disable once PossibleIntendedRethrow
                throw ex;
            }
            finally
            {
                // 关闭连接
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                    da?.Dispose();
                    conn.Dispose();
                }
            }
            return ds;
        }
        #endregion

        #region [3、将DataTable数据导入到Excel中]
        /// <summary>
        /// 将DataTable数据导入到excel中
        /// </summary>
        /// <param name="data">要导入的数据</param>
        /// <param name="isColumnWritten">DataTable的列名是否要导入</param>
        /// <param name="sheetName">要导入的excel的sheet的名称</param>
        /// <returns>导入数据行数(包含列名那一行)</returns>
        public int DataTableToExcel(DataTable data, string sheetName, bool isColumnWritten)
        {
            if (string.IsNullOrEmpty(_fileName)) return -1;
            _fs = new FileStream(_fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            var index = _fileName.LastIndexOf('.');
            var extensionName = _fileName.Substring(index);
            // 2007版本忽略文件扩展名大小写
            if (string.Equals(extensionName, ".xlsx", StringComparison.CurrentCultureIgnoreCase))
            {
                _workbook = new XSSFWorkbook();
            }
            // 2003版本忽略文件扩展名大小写
            else if (string.Equals(extensionName, ".xls", StringComparison.CurrentCultureIgnoreCase))
            {
                _workbook = new HSSFWorkbook();
            }
            var cellStyle = _workbook.CreateCellStyle();
            //为避免日期格式被Excel自动替换，所以设定 format 为 『@』 表示一率当成text來看
            cellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("@");

            try
            {
                ISheet sheet;
                if (_workbook != null)
                {
                    sheet = _workbook.CreateSheet(sheetName);
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
                //写入到excel
                _workbook.Write(_fs);
                return count;
            }
            finally
            {
                //释放资源
                _fs.Close();
            }
        }
        #endregion

        #region [4、将Excel中的数据导入到DataTable中]
        /// <summary>
        /// 将excel中的数据导入到DataTable中
        /// </summary>
        /// <param name="sheetName">excel工作薄sheet的名称</param>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名</param>
        /// <returns>返回的DataTable</returns>
        public DataTable ExcelToDataTable(string sheetName, bool isFirstRowColumn)
        {
            DataTable data = new DataTable();
            if (string.IsNullOrEmpty(_fileName)) return data;
            try
            {
                _fs = new FileStream(_fileName, FileMode.Open, FileAccess.Read);
                var index = _fileName.LastIndexOf('.');
                var extensionName = _fileName.Substring(index);
                if (string.Equals(extensionName, ".xlsx", StringComparison.CurrentCultureIgnoreCase))
                {
                    _workbook = new XSSFWorkbook(_fs);
                }
                else if (string.Equals(extensionName, ".xls", StringComparison.CurrentCultureIgnoreCase))
                {
                    _workbook = new HSSFWorkbook(_fs);
                }

                ISheet sheet;
                if (sheetName != null)
                {
                    //如果没有找到指定的sheetName对应的sheet，则尝试获取第一个sheet
                    sheet = _workbook.GetSheet(sheetName) ?? _workbook.GetSheetAt(0);
                }
                else
                {
                    sheet = _workbook.GetSheetAt(0);
                }
                if (sheet != null)
                {
                    IRow firstRow = sheet.GetRow(0);
                    //一行最后一个cell的编号 即总的列数
                    int cellCount = firstRow.LastCellNum;

                    int startRow;
                    if (isFirstRowColumn)
                    {
                        for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                        {
                            ICell cell = firstRow.GetCell(i);
                            string cellValue = cell?.StringCellValue;
                            if (cellValue != null)
                            {
                                DataColumn column = new DataColumn(cellValue);
                                data.Columns.Add(column);
                            }
                        }
                        startRow = sheet.FirstRowNum + 1;
                    }
                    else
                    {
                        startRow = sheet.FirstRowNum;
                    }

                    //最后一列的标号
                    int rowCount = sheet.LastRowNum;
                    for (int i = startRow; i <= rowCount; ++i)
                    {
                        IRow row = sheet.GetRow(i);
                        //没有数据的行默认是null
                        if (row == null) continue;

                        DataRow dataRow = data.NewRow();
                        for (int j = row.FirstCellNum; j < cellCount; ++j)
                        {
                            if (row.GetCell(j) != null)
                                dataRow[j] = row.GetCell(j);
                        }
                        data.Rows.Add(dataRow);
                    }
                }
                return data;
            }
            finally
            {
                _fs.Close();
            }
        }
        #endregion

        #region [5、excel导入DataSet中]
        /// <summary>
        /// excel导入DataSet中
        /// </summary>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名</param>
        /// <author>FreshMan</author>
        /// <creattime>2015-11-19</creattime>
        /// <returns>excel中的每一个sheet作为一个DataTable添加到DataSet中并返回</returns>
        public DataSet ExcelToDataSet(bool isFirstRowColumn)
        {
            var dataSet = new DataSet();
            if (string.IsNullOrEmpty(_fileName)) return dataSet;

            try
            {
                #region 读取数据文件流

                _fs = new FileStream(_fileName, FileMode.Open, FileAccess.Read);
                // 2007版本
                if (_fileName.EndsWith(".xlsx")) _workbook = new XSSFWorkbook(_fs);
                // 2003版本
                else if (_fileName.EndsWith(".xls")) _workbook = new HSSFWorkbook(_fs);
                else return dataSet;
                #endregion

                #region 循环读取工作表中的数据
                //获得表总数
                var sheetsNum = _workbook.NumberOfSheets;
                if (sheetsNum <= 0) return dataSet;
                for (var sheetItem = 0; sheetItem < sheetsNum; sheetItem++)
                {
                    var data = new DataTable();
                    var sheet = _workbook.GetSheetAt(sheetItem);
                    if (sheet == null) continue;
                    data.TableName = sheet.SheetName;
                    IRow firstRow = sheet.GetRow(0);
                    //一行最后一个cell的编号 即总的列数
                    int cellCount = firstRow.LastCellNum;
                    int startRow;

                    #region 创建数据列

                    if (isFirstRowColumn)
                    {
                        for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                        {
                            var column = new DataColumn(firstRow.GetCell(i).StringCellValue);
                            data.Columns.Add(column);
                        }
                        startRow = sheet.FirstRowNum + 1;
                    }
                    else
                    {
                        for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                        {
                            var column = new DataColumn();
                            data.Columns.Add(column);
                        }
                        startRow = sheet.FirstRowNum;
                    }

                    #endregion

                    #region 填充数据

                    //最后一列的标号
                    var rowCount = sheet.LastRowNum;
                    for (var i = startRow; i <= rowCount; ++i)
                    {
                        var row = sheet.GetRow(i);
                        //没有数据的行默认是null
                        if (row == null) continue;

                        var dataRow = data.NewRow();
                        for (int j = row.FirstCellNum; j < cellCount; ++j)
                        {
                            if (row.GetCell(j) == null) continue;
                            var str = row.GetCell(j).ToString();
                            dataRow[j] = str;
                        }
                        data.Rows.Add(dataRow);
                    }
                    #endregion

                    dataSet.Tables.Add(data);
                }
                #endregion

                return dataSet;
            }
            catch (Exception)
            {
                return dataSet;
            }
        }
        #endregion
    }
}
