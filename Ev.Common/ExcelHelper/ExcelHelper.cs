/**============================================================
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

namespace Ev.Common.ExcelHelper
{
    public class ExcelHelper
    {
        /// <summary>
        /// 读取Excel表中的数据到DataSet中
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>数据集合DataSet</returns>
        public static DataSet ReadExcelToDataSet(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return null;
            string tblName = filePath.Substring(filePath.LastIndexOf("\\", StringComparison.Ordinal) + 1);
            string connStr;
            //获取文件扩展名称
            string fileType = Path.GetExtension(tblName);
            if (string.IsNullOrEmpty(fileType)) return null;

            if (fileType == ".xls") connStr = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + filePath + ";" + ";Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1\"";//HDR=YES;第一列作为标题 IMEX=0不读为字符串
            else connStr = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + filePath + ";" + ";Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1\"";
            const string sqlF = "Select * FROM [{0}]";

            OleDbConnection conn = null;
            OleDbDataAdapter da = null;

            DataSet ds = new DataSet();
            try
            {
                // 初始化连接，并打开
                conn = new OleDbConnection(connStr);
                conn.Open();

                var dtSheetName = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
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
            catch (Exception)
            {
                return null;
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
    }
}
