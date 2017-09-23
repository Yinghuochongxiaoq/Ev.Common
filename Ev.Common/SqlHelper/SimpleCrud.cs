/**============================================================
* 命名空间: Ev.Common.SqlHelper
*
* 功 能： N/A
* 类 名： SimpleCrud
*
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
* V0.01 2017/7/7 18:01:44 FreshMan 初版
*
* Copyright (c) 2017 Lir Corporation. All rights reserved.
*==============================================================
*==此技术信息为本公司机密信息,未经本公司书面同意禁止向第三方披露==
*==版权所有：重庆慧都科技有限公司                             ==
*==============================================================
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Ev.Common.CommonModel;

namespace Ev.Common.SqlHelper
{
    /// <summary>
    /// 简单类型处理
    /// </summary>
    public static partial class SimpleCrud
    {
        #region [1、全局变量]

        /// <summary>
        /// 链接对象
        /// </summary>
        private static SqlConnection _connection;

        /// <summary>
        /// 链接字符串
        /// </summary>
        private static string _connectionString;
        #endregion

        #region [2、共有方法]

        /// <summary>
        /// 设置当前链接字符串
        /// </summary>
        /// <param name="selfConnectionString"></param>
        /// <returns></returns>
        public static bool SetConnectionString(string selfConnectionString)
        {
            if (string.IsNullOrEmpty(selfConnectionString)) return false;
            _connectionString = selfConnectionString;
            return true;
        }

        /// <summary>
        /// 获取当前链接字符串
        /// </summary>
        /// <returns></returns>
        public static string GetConnectionString()
        {
            return _connectionString;
        }

        /// <summary>
        /// 获取链接对象，优先考虑自定义链接字符串
        /// </summary>
        public static SqlConnection Connection
        {
            get
            {
                _connectionString = string.IsNullOrEmpty(_connectionString)
                    ? ConfigurationManager.AppSettings["ConstrSQL"]
                    : _connectionString;

                if (_connection == null)
                {
                    _connection = new SqlConnection(_connectionString);
                    _connection.Open();
                }
                else if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                }
                else if (_connection.State == ConnectionState.Broken)
                {
                    _connection.Close();
                    _connection.Open();
                }
                return _connection;
            }
        }

        /// <summary>
        /// 执行sql
        /// </summary>
        /// <param name="safeSql"></param>
        /// <returns></returns>
        public static int ExecuteCommand(string safeSql)
        {
            try
            {
                SqlCommand cmd = new SqlCommand(safeSql, Connection);
                int result = cmd.ExecuteNonQuery();
                return result;
            }
            catch (Exception ex)
            {
                var exception =
                    new Exception(
                        $" Execute Sql command:{safeSql} maybe error,please check.Exception error message is:{ex.Message},innerExcetpion error message is :{ex.InnerException?.Message}",
                        ex);
                throw exception;
            }
        }

        /// <summary>
        /// 带参数的执行命令
        /// </summary>
        /// <param name="sql">SQL命令</param>
        /// <param name="values">返回VALUE值</param>
        /// <returns></returns>
        public static int ExecuteCommand(string sql, params SqlParameter[] values)
        {
            try
            {
                SqlCommand cmd = new SqlCommand(sql, Connection);
                cmd.Parameters.AddRange(values);
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                var exception =
                    new Exception(
                        $" Execute Sql command:{sql} maybe error,please check.Exception error message is:{ex.Message},innerExcetpion error message is :{ex.InnerException?.Message}",
                        ex);
                throw exception;
            }
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="values"></param>
        /// <param name="transaction"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public static int ExecuteCommand(string sql, IEnumerable<SqlParameter> values, bool transaction = false, int? timeOut = null)
        {
            SqlTransaction tranProducts = null;
            try
            {
                SqlCommand cmd = new SqlCommand(sql, Connection);
                if (transaction)
                {
                    tranProducts = Connection.BeginTransaction();
                }
                if (timeOut != null && timeOut > 0)
                {
                    cmd.CommandTimeout = (int)timeOut;
                }
                cmd.Parameters.AddRange(values.ToArray());
                cmd.Transaction = tranProducts;
                var result = cmd.ExecuteNonQuery();
                if (transaction)
                {
                    tranProducts.Commit();
                }
                return result;
            }
            catch (Exception ex)
            {
                if (transaction)
                {
                    tranProducts?.Rollback();
                }
                var exception =
                    new Exception(
                        $" Execute Sql command:{sql} maybe error,please check.Exception error message is:{ex.Message},innerExcetpion error message is :{ex.InnerException?.Message}",
                        ex);
                throw exception;
            }
        }

        /// <summary>
        /// 返回影响记录数
        /// </summary>
        /// <param name="safeSql"></param>
        /// <returns></returns>
        public static object GetScalar(string safeSql)
        {
            try
            {
                SqlCommand cmd = new SqlCommand(safeSql, Connection);
                var result = cmd.ExecuteScalar();
                return result;
            }
            catch (Exception ex)
            {
                var exception =
                    new Exception(
                        $" Execute Sql command:{safeSql} maybe error,please check.Exception error message is:{ex.Message},innerExcetpion error message is :{ex.InnerException?.Message}",
                        ex);
                throw exception;
            }
        }

        /// <summary>
        /// 执行只返回单行单列的值
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static object GetScalar(string sql, params SqlParameter[] values)
        {
            try
            {
                SqlCommand cmd = new SqlCommand(sql, Connection);
                cmd.Parameters.AddRange(values);
                var result = cmd.ExecuteScalar();
                return result;
            }
            catch (Exception ex)
            {
                var exception =
                    new Exception(
                        $" Execute Sql command:{sql} maybe error,please check.Exception error message is:{ex.Message},innerExcetpion error message is :{ex.InnerException?.Message}",
                        ex);
                throw exception;
            }

        }

        /// <summary>
        /// 获取一个读取器
        /// </summary>
        /// <param name="safeSql"></param>
        /// <returns></returns>
        public static SqlDataReader GetReader(string safeSql)
        {
            try
            {
                SqlCommand cmd = new SqlCommand(safeSql, Connection);
                SqlDataReader reader = cmd.ExecuteReader();
                return reader;
            }
            catch (Exception ex)
            {
                var exception =
                    new Exception(
                        $" Execute Sql command:{safeSql} maybe error,please check.Exception error message is:{ex.Message},innerExcetpion error message is :{ex.InnerException?.Message}",
                        ex);
                throw exception;
            }
        }

        /// <summary>
        /// 获取一个读取器
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static SqlDataReader GetReader(string sql, params SqlParameter[] values)
        {
            try
            {
                SqlCommand cmd = new SqlCommand(sql, Connection);
                cmd.Parameters.AddRange(values);
                SqlDataReader reader = cmd.ExecuteReader();
                return reader;
            }
            catch (Exception ex)
            {
                var exception =
                    new Exception(
                        $" Execute Sql command:{sql} maybe error,please check.Exception error message is:{ex.Message},innerExcetpion error message is :{ex.InnerException?.Message}",
                        ex);
                throw exception;
            }
        }

        /// <summary>
        /// 无参数sql返回DataTable
        /// </summary>
        /// <param name="safeSql"></param>
        /// <returns></returns>
        public static DataTable GetDataTable(string safeSql)
        {
            try
            {
                DataSet ds = new DataSet();
                SqlCommand cmd = new SqlCommand(safeSql, Connection);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(ds);
                return ds.Tables[0];
            }
            catch (Exception ex)
            {
                var exception =
                    new Exception(
                        $" Execute Sql command:{safeSql} maybe error,please check.Exception error message is:{ex.Message},innerExcetpion error message is :{ex.InnerException?.Message}",
                        ex);
                throw exception;
            }
        }

        /// <summary>
        /// 带参数sql返回DataTable
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static DataTable GetDataTable(string sql, params SqlParameter[] values)
        {
            try
            {
                DataSet ds = new DataSet();
                SqlCommand cmd = new SqlCommand(sql, Connection);
                cmd.Parameters.AddRange(values);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(ds);
                return ds.Tables[0];
            }
            catch (Exception ex)
            {
                var exception =
                    new Exception(
                        $" Execute Sql command:{sql} maybe error,please check.Exception error message is:{ex.Message},innerExcetpion error message is :{ex.InnerException?.Message}",
                        ex);
                throw exception;
            }
        }

        /// <summary>
        /// 获得数据集
        /// </summary>
        public static DataSet GetDataSet(string sql, string table)
        {
            try
            {
                DataSet ds = new DataSet();
                SqlCommand cmd = new SqlCommand(sql, Connection);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(ds, table);
                return ds;
            }
            catch (Exception ex)
            {
                var exception =
                    new Exception(
                        $" Execute Sql command:{sql} maybe error,please check.Exception error message is:{ex.Message},innerExcetpion error message is :{ex.InnerException?.Message}",
                        ex);
                throw exception;
            }
        }

        /// <summary>
        /// 获取数据集
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="table"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static DataSet GetDataSet(string sql, string table, params SqlParameter[] values)
        {
            try
            {
                DataSet ds = new DataSet();
                SqlCommand cmd = new SqlCommand(sql, Connection);
                cmd.Parameters.AddRange(values);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(ds, table);
                return ds;
            }
            catch (Exception ex)
            {
                var exception =
                    new Exception(
                        $" Execute Sql command:{sql} maybe error,please check.Exception error message is:{ex.Message},innerExcetpion error message is :{ex.InnerException?.Message}",
                        ex);
                throw exception;
            }
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="values"></param>
        /// <param name="transaction"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public static dynamic Query(string sql, IEnumerable<SqlParameter> values, bool transaction = false, int? timeOut = null)
        {
            SqlTransaction tranProducts = null;
            try
            {
                SqlCommand cmd = new SqlCommand(sql, Connection);
                if (transaction)
                {
                    tranProducts = Connection.BeginTransaction();
                }
                if (timeOut != null && timeOut > 0)
                {
                    cmd.CommandTimeout = (int)timeOut;
                }
                cmd.Parameters.AddRange(values.ToArray());
                cmd.Transaction = tranProducts;
                var result = cmd.ExecuteScalar();
                if (transaction)
                {
                    tranProducts.Commit();
                }
                return result;
            }
            catch (Exception ex)
            {
                if (transaction)
                {
                    tranProducts?.Rollback();
                }
                var exception =
                    new Exception(
                        $" Execute Sql command:{sql} maybe error,please check.Exception error message is:{ex.Message},innerExcetpion error message is :{ex.InnerException?.Message}",
                        ex);
                throw exception;
            }
        }
        #endregion

        #region [3、删除表数据SQL Code]

        /// <summary>
        /// 根据表或视图名删除一个表或视图数据
        /// </summary>
        /// <param name="tableNameList">表或视图名称集合</param>
        /// <param name="dataBase">数据库名称，默认为当前链接数据库</param>
        /// <param name="isView">是否是视图：true：是；false：否（默认）</param>
        /// <param name="type">1:delete;0:drop</param>
        /// <author>FreshMan</author>
        /// <creattime>2017-09-06</creattime>
        /// <returns>删除字符串</returns>
        private static string GetDeleteOrDropDataTableSqlByName(List<string> tableNameList, string dataBase, bool isView, int type)
        {
            if (tableNameList == null || tableNameList.Count < 1) return string.Empty;
            var hadDeleteTable = new Dictionary<string, int>();
            var historyDictionary = new Dictionary<string, int>();
            StringBuilder resulteBuilder = new StringBuilder();
            var typeSql = type == 1 ? " DELETE FROM " : (type == 0 ? " DROP TABLE " : string.Empty);
            foreach (var tableName in tableNameList.Distinct().Where(tableName => !string.IsNullOrEmpty(tableName)))
            {
                if (historyDictionary.ContainsKey(tableName)) continue;
                var referencedTableList = GetDeleteTableNameList(tableName, historyDictionary);
                if (!string.IsNullOrEmpty(dataBase))
                {
                    dataBase = dataBase + ".dbo.";
                }
                string itemDeleteSql = ";IF EXISTS ( SELECT * FROM " + dataBase + " sysobjects WHERE name = '{0}' AND type = '" + (isView ? "V" : "U") + "') " + typeSql + dataBase + "[{0}] ;";
                if (referencedTableList == null || referencedTableList.Count < 1) continue;
                foreach (var tempString in referencedTableList)
                {
                    if (hadDeleteTable.ContainsKey(tempString))
                    {
                        hadDeleteTable[tempString]++;
                    }
                    else
                    {
                        hadDeleteTable.Add(tempString, 1);
                        resulteBuilder.AppendFormat(itemDeleteSql, tempString);
                    }
                }
            }
            return resulteBuilder.ToString();
        }

        /// <summary>
        /// 根据表或视图名删除一个表或视图数据
        /// </summary>
        /// <param name="tableName">表或视图名称</param>
        /// <param name="dataBase">数据库名称，默认为当前链接数据库</param>
        /// <param name="isView">是否是视图：true：是；false：否（默认）</param>
        /// <author>FreshMan</author>
        /// <creattime>2017-09-5</creattime>
        /// <returns>删除字符串</returns>
        public static string GetDeleteDataTableSqlByName(string tableName, string dataBase = null, bool isView = false)
        {
            return GetDeleteOrDropDataTableSqlByName(new List<string> { tableName }, dataBase, isView, 1);
        }

        /// <summary>
        /// 根据表或视图名删除一个表或视图数据
        /// </summary>
        /// <param name="tableNameList">表或视图名称集合</param>
        /// <param name="dataBase">数据库名称，默认为当前链接数据库</param>
        /// <param name="isView">是否是视图：true：是；false：否（默认）</param>
        /// <author>FreshMan</author>
        /// <creattime>2017-09-06</creattime>
        /// <returns>删除字符串</returns>
        public static string GetDeleteDataTableSqlByName(List<string> tableNameList, string dataBase = null, bool isView = false)
        {
            return GetDeleteOrDropDataTableSqlByName(tableNameList, dataBase, isView, 1);
        }

        /// <summary>
        /// 根据表或视图名删除一个表或视图结构
        /// </summary>
        /// <param name="tableName">表或视图名称集合</param>
        /// <param name="dataBase">数据库名称，默认为当前链接数据库</param>
        /// <param name="isView">是否是视图：true：是；false：否（默认）</param>
        /// <author>FreshMan</author>
        /// <creattime>2017-09-06</creattime>
        /// <returns>删除字符串</returns>
        public static string GetDropDataTableSqlByName(string tableName, string dataBase = null, bool isView = false)
        {
            return GetDeleteOrDropDataTableSqlByName(new List<string> { tableName }, dataBase, isView, 1);
        }

        /// <summary>
        /// 根据表或视图名删除一个表或视图结构
        /// </summary>
        /// <param name="tableNameList">表或视图名称集合</param>
        /// <param name="dataBase">数据库名称，默认为当前链接数据库</param>
        /// <param name="isView">是否是视图：true：是；false：否（默认）</param>
        /// <author>FreshMan</author>
        /// <creattime>2017-09-06</creattime>
        /// <returns>删除字符串</returns>
        public static string GetDropDataTableSqlByName(List<string> tableNameList, string dataBase = null, bool isView = false)
        {
            return GetDeleteOrDropDataTableSqlByName(tableNameList, dataBase, isView, 0);
        }

        /// <summary>
        /// 获取引用图
        /// <author>FreshMan</author>
        /// <creattime>2017-09-06</creattime>
        /// </summary>        private static Dictionary<string, List<string>> GetReferencedMap()
        {
            Dictionary<string, List<string>> tableRefencedModelDictionary = new Dictionary<string, List<string>>();
            string sqlCmd = $@"
SELECT
    object_name(constraint_object_id) ForeignKey,
	object_name(parent_object_id) TableName,
	col_name(
        parent_object_id,
        parent_column_id
    ) ForeignKeyCell,
	object_name(referenced_object_id) ReferencedTableName,
	col_name(
        referenced_object_id,
        referenced_column_id
    ) ReferencedCell
FROM
    sys.foreign_key_columns ";
            var resulteInfo = new List<ReferencedModel>();
            var dr = GetReader(sqlCmd);
            while (dr.Read())
            {
                var tempModel = new ReferencedModel
                {
                    ForeignKey = (string)dr["ForeignKey"],
                    ForeignKeyCell = (string)dr["ForeignKeyCell"],
                    ReferencedCell = (string)dr["ReferencedCell"],
                    ReferencedTableName = (string)dr["ReferencedTableName"],
                    TableName = (string)dr["TableName"]
                };
                resulteInfo.Add(tempModel);
                if (tableRefencedModelDictionary.ContainsKey(tempModel.ReferencedTableName)) continue;
                tableRefencedModelDictionary.Add(tempModel.ReferencedTableName, new List<string>());
            }
            dr.Close();

            //形成有向图
            foreach (var rowModel in resulteInfo.Where(rowModel => rowModel.ReferencedTableName != rowModel.TableName))
            {
                tableRefencedModelDictionary[rowModel.ReferencedTableName].Add(rowModel.TableName);
            }
            return tableRefencedModelDictionary;
        }

        /// <summary>
        /// 获得被引用集合
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="historyDictionary">访问链路</param>
        /// <author>FreshMan</author>
        /// <creattime>2017-09-06</creattime>
        /// <returns></returns>
        private static List<string> GetDeleteTableNameList(string tableName, Dictionary<string, int> historyDictionary)
        {
            if (string.IsNullOrEmpty(tableName)) return null;
            var referencedMap = GetReferencedMap();
            if (!referencedMap.ContainsKey(tableName))
            {
                historyDictionary.Add(tableName, 1);
                return new List<string> { tableName };
            }
            var nodeSet = new HashSet<string>();
            var resultList = TraversingGraph(referencedMap, tableName, historyDictionary, nodeSet);
            return resultList;
        }

        /// <summary>
        /// 递归完成深度遍历图
        /// </summary>
        /// <param name="sourceDictionary">原始结点数据值</param>
        /// <param name="nodeName">当前结点</param>
        /// <param name="historyDictionary">倒序叶子节点</param>
        /// <param name="nodeSet">记录是否访问过</param>
        /// <returns></returns>
        private static List<string> TraversingGraph(Dictionary<string, List<string>> sourceDictionary, string nodeName, Dictionary<string, int> historyDictionary, HashSet<string> nodeSet)
        {
            var result = new List<string>();
            if (!nodeSet.Add(nodeName)) return result;
            //是否已经访问过
            if (historyDictionary.ContainsKey(nodeName)) return result;
            //出度为0
            if (!sourceDictionary.ContainsKey(nodeName))
            {
                //标记已经访问
                historyDictionary.Add(nodeName, 1);
                result.Add(nodeName);
                return result;
            }
            //出度大于0
            for (int i = 0; i < sourceDictionary[nodeName].Count; i++)
            {
                var nextNodeName = sourceDictionary[nodeName][i];
                var recurrenceList = TraversingGraph(sourceDictionary, nextNodeName, historyDictionary, nodeSet);
                if (recurrenceList != null && recurrenceList.Any()) result.AddRange(recurrenceList);
            }
            result.Add(nodeName);
            //标记已经访问
            historyDictionary.Add(nodeName, 1);
            return result;
        }
        #endregion
    }
}
