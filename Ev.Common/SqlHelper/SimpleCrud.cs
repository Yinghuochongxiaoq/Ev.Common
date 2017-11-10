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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Xpo;
using Ev.Common.CommonModel;
using Ev.Common.DataConvert;

namespace Ev.Common.SqlHelper
{
    /// <summary>
    /// 简单类型处理
    /// <para>数据库链接字符串名称为：ConstrSQL,需存放于AppSettings节点</para>
    /// <para>或者使用<see cref="SetConnectionString"/>重新设置全局链接字符串变量</para>
    /// </summary>
    public static partial class SimpleCrud
    {
        private static string _s;
        static SimpleCrud()
        {
            _s = string.IsNullOrEmpty(_s)
                    ? ConfigurationManager.AppSettings["ConstrSQL"]
                    : _s;
        }

        /// <summary>
        /// 设置当前链接字符串
        /// </summary>
        /// <returns></returns>
        public static bool SetConnectionString(string selfConnectionString)
        {
            if (string.IsNullOrEmpty(selfConnectionString)) return false;
            _s = selfConnectionString;
            return true;
        }

        /// <summary>
        /// 获取当前链接字符串
        /// </summary>
        /// <returns></returns>
        public static string GetConnectionString()
        {
            return _s;
        }

        /// <summary>
        /// 执行sql
        /// </summary>
        /// <returns>执行结果影响行数</returns>
        public static int ExecuteCommand(string safeSql, string connectionString = null)
        {
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = _s;
                }
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(safeSql, conn);
                    int a = cmd.ExecuteNonQuery();
                    return a;
                }
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
        /// <returns>执行结果影响行数</returns>
        public static int ExecuteCommand(string sql, SqlParameter[] values, string connectionString = null)
        {
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = _s;
                }
                using (var c = new SqlConnection(connectionString))
                {
                    c.Open();
                    SqlCommand d = new SqlCommand(sql, c);
                    if (values.Length > 0)
                    {
                        d.Parameters.AddRange(values);
                    }
                    return d.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                var f =
                    new Exception(
                        $" Execute Sql command:{sql} maybe error,please check.Exception error message is:{ex.Message},innerExcetpion error message is :{ex.InnerException?.Message}",
                        ex);
                throw f;
            }
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <returns>执行结果影响行数</returns>
        public static int ExecuteCommand(string sql, IEnumerable<SqlParameter> values, bool transaction = false, int? timeOut = null, string connectionString = null)
        {
            SqlTransaction t = null;
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = _s;
                }
                using (var a = new SqlConnection(connectionString))
                {
                    a.Open();
                    SqlCommand b = new SqlCommand(sql, a);
                    if (transaction)
                    {
                        t = a.BeginTransaction();
                        b.Transaction = t;
                    }
                    if (timeOut != null && timeOut > 0)
                    {
                        b.CommandTimeout = (int)timeOut;
                    }
                    var paramtersList = values.ToArray();
                    if (paramtersList.Length > 0)
                    {
                        b.Parameters.AddRange(paramtersList);
                    }
                    var c = b.ExecuteNonQuery();
                    if (transaction)
                    {
                        t.Commit();
                    }
                    return c;
                }

            }
            catch (Exception ex)
            {
                if (transaction)
                {
                    try
                    {
                        t?.Rollback();
                    }
                    catch (Exception extwo)
                    {
                        var tempEx =
                    new Exception(
                        $" Execute Sql command:{sql} maybe error,please check.Exception error message is:{ex.Message},innerExcetpion error message is :{extwo.InnerException?.Message}",
                        extwo);
                        throw tempEx;
                    }

                }
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
        /// <returns>执行结果影响行数</returns>
        public static bool ExecuteCommand(List<string> sql, IEnumerable<SqlParameter> values, bool transaction = false, int? timeOut = null, string connectionString = null)
        {

            SqlTransaction t = null;
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = _s;
                }
                using (var c = new SqlConnection(connectionString))
                {
                    c.Open();
                    SqlCommand d = new SqlCommand { Connection = c };
                    if (transaction)
                    {
                        t = c.BeginTransaction();
                        d.Transaction = t;
                    }
                    foreach (var tempSql in sql)
                    {
                        if (transaction)
                        {
                            d.Transaction = t;
                        }
                        d.CommandText = tempSql;
                        d.ExecuteNonQuery();
                    }
                    if (timeOut != null && timeOut > 0)
                    {
                        d.CommandTimeout = (int)timeOut;
                    }
                    var paramtersList = values.ToArray();
                    if (paramtersList.Length > 0)
                    {
                        d.Parameters.AddRange(paramtersList);
                    }
                    if (transaction)
                    {
                        t.Commit();
                    }
                    return true;
                }

            }
            catch (Exception ex)
            {
                if (transaction)
                {
                    try
                    {
                        t?.Rollback();
                    }
                    catch (Exception extwo)
                    {
                        var tempEx =
                    new Exception(
                        $" Execute Sql command:{sql} maybe error,please check.Exception error message is:{ex.Message},innerExcetpion error message is :{extwo.InnerException?.Message}",
                        extwo);
                        throw tempEx;
                    }

                }
                var exception =
                    new Exception(
                        $" Execute Sql command:{sql} maybe error,please check.Exception error message is:{ex.Message},innerExcetpion error message is :{ex.InnerException?.Message}",
                        ex);
                throw exception;
            }
        }

        /// <summary>
        /// 执行只返回单行单列的值
        /// </summary>
        /// <returns>执行只返回单行单列的值</returns>
        public static object GetScalar(string safeSql, string connectionString = null)
        {
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = _s;
                }
                using (var c = new SqlConnection(connectionString))
                {
                    c.Open();
                    SqlCommand d = new SqlCommand(safeSql, c);
                    var result = d.ExecuteScalar();
                    return result;
                }
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
        /// <returns>执行只返回单行单列的值</returns>
        public static object GetScalar(string sql, SqlParameter[] values, string connectionString = null)
        {
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = _s;
                }
                using (var c = new SqlConnection(connectionString))
                {
                    c.Open();
                    SqlCommand d = new SqlCommand(sql, c);
                    if (values.Length > 0)
                    {
                        d.Parameters.AddRange(values);
                    }
                    var result = d.ExecuteScalar();
                    return result;
                }
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
        /// 执行只返回单行单列的值
        /// </summary>
        /// <returns></returns>
        public static dynamic GetScalar(string sql, IEnumerable<SqlParameter> values, bool transaction = false, int? timeOut = null, string connectionString = null)
        {
            SqlTransaction t = null;
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = _s;
                }
                using (var c = new SqlConnection(connectionString))
                {
                    c.Open();
                    SqlCommand cc = new SqlCommand(sql, c);
                    if (transaction)
                    {
                        t = c.BeginTransaction();
                    }
                    if (timeOut != null && timeOut > 0)
                    {
                        cc.CommandTimeout = (int)timeOut;
                    }
                    var paramtersList = values.ToArray();
                    if (paramtersList.Length > 0)
                    {
                        cc.Parameters.AddRange(paramtersList);
                    }
                    cc.Transaction = t;
                    var result = cc.ExecuteScalar();
                    if (transaction)
                    {
                        t.Commit();
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                if (transaction)
                {
                    t?.Rollback();
                }
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
        /// <returns>获取一个读取器</returns>
        public static SqlDataReader GetReader(string safeSql, string connectionString = null)
        {
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = _s;
                }
                var c = new SqlConnection(connectionString);
                c.Open();
                SqlCommand d = new SqlCommand(safeSql, c);
                SqlDataReader e = d.ExecuteReader();
                return e;
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
        /// <returns>获取一个读取器</returns>
        public static SqlDataReader GetReader(string sql, SqlParameter[] values, string connectionString = null)
        {
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = _s;
                }
                var c = new SqlConnection(connectionString);
                c.Open();
                SqlCommand d = new SqlCommand(sql, c);
                if (values.Length > 0)
                {
                    d.Parameters.AddRange(values);
                }
                SqlDataReader reader = d.ExecuteReader();
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
        /// <returns>结果集合</returns>
        public static DataTable GetDataTable(string safeSql, string connectionString = null)
        {
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = _s;
                }
                using (var cc = new SqlConnection(connectionString))
                {
                    cc.Open();
                    DataSet d = new DataSet();
                    SqlCommand adc = new SqlCommand(safeSql, cc);
                    SqlDataAdapter da = new SqlDataAdapter(adc);
                    da.Fill(d);
                    return d.Tables[0];
                }
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
        /// <returns>结果集合</returns>
        public static DataTable GetDataTable(string sql, SqlParameter[] values, string connectionString = null)
        {
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = _s;
                }
                using (var zz = new SqlConnection(connectionString))
                {
                    zz.Open();
                    DataSet fade = new DataSet();
                    SqlCommand gew = new SqlCommand(sql, zz);
                    if (values.Length > 0)
                    {
                        gew.Parameters.AddRange(values);
                    }
                    SqlDataAdapter da = new SqlDataAdapter(gew);
                    da.Fill(fade);
                    return fade.Tables[0];
                }
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
        /// <returns>结果集合</returns>
        public static DataSet GetDataSet(string sql, string table, string connectionString = null)
        {
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = _s;
                }
                using (var ade = new SqlConnection(connectionString))
                {
                    ade.Open();
                    DataSet aecs = new DataSet();
                    SqlCommand rwe = new SqlCommand(sql, ade);
                    SqlDataAdapter da = new SqlDataAdapter(rwe);
                    da.Fill(aecs, table);
                    return aecs;
                }
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
        /// <returns></returns>
        public static DataSet GetDataSet(string sql, string table, SqlParameter[] values, string connectionString = null)
        {
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = _s;
                }
                using (var cadf = new SqlConnection(connectionString))
                {
                    cadf.Open();
                    DataSet zabg = new DataSet();
                    SqlCommand wq5 = new SqlCommand(sql, cadf);
                    if (values.Length > 0)
                    {
                        wq5.Parameters.AddRange(values);
                    }
                    SqlDataAdapter da = new SqlDataAdapter(wq5);
                    da.Fill(zabg, table);
                    return zabg;
                }
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
        /// 批量更新数据
        /// </summary>
        public static void BulkUpdate(DataTable table, string connectionString = null)
        {
            if (table == null || table.Rows.Count < 1) return;
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = _s;
            }
            using (SqlConnection gytr = new SqlConnection(connectionString))
            {
                SqlCommand q32Tu = gytr.CreateCommand();
                q32Tu.CommandTimeout = 30;
                q32Tu.CommandType = CommandType.Text;
                SqlDataAdapter p0Oi = new SqlDataAdapter(q32Tu);
                SqlCommandBuilder miu78Y = new SqlCommandBuilder(p0Oi);
                miu78Y.ConflictOption = ConflictOption.OverwriteChanges;
                try
                {
                    gytr.Open();
                    p0Oi.SelectCommand.Transaction = gytr.BeginTransaction();
                    p0Oi.Update(table);
                    p0Oi.SelectCommand.Transaction.Commit();
                }
                catch (Exception)
                {
                    p0Oi.SelectCommand?.Transaction?.Rollback();
                    throw;
                }
                finally
                {
                    gytr.Close();
                    gytr.Dispose();
                }
            }
        }

        /// <summary> 
        /// 大批量插入数据 
        /// 已采用整体事物控制 
        /// </summary> 
        /// <param name="timeOut">超时时间，单位秒，默认60s</param>
        public static void BulkCopy(string tableName, DataTable dt, string connectionString = null, int timeOut = 60)
        {
            if (string.IsNullOrEmpty(tableName) || dt == null || dt.Rows.Count < 0) return;
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = _s;
            }
            using (SqlConnection wert = new SqlConnection(connectionString))
            {
                wert.Open();
                using (SqlTransaction hrb = wert.BeginTransaction())
                {
                    using (SqlBulkCopy kiol = new SqlBulkCopy(wert, SqlBulkCopyOptions.Default, hrb))
                    {
                        kiol.BulkCopyTimeout = 60;
                        kiol.DestinationTableName = tableName;
                        try
                        {
                            foreach (DataColumn col in dt.Columns)
                            {
                                kiol.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                            }
                            kiol.WriteToServer(dt);
                            hrb.Commit();
                        }
                        catch (Exception)
                        {
                            hrb.Rollback();
                            throw;
                        }
                        finally
                        {
                            wert.Close();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 大批量插入数据
        /// </summary>
        /// <param name="timeOut">超时时间，单位秒，默认60s</param>
        public static void BulkCopy(DataTable dt, string connectionString = null, int timeOut = 60)
        {
            BulkCopy(dt.TableName, dt, connectionString, timeOut);
        }

        /// <summary>
        /// 批量插入数据
        /// </summary>
        /// <param name="timeOut">超时时间，单位秒，默认60s</param>
        public static void BulkCopy(DataSet ds, string connectionString = null, int timeOut = 60)
        {
            if (ds == null || ds.Tables.Count < 1) return;
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = _s;
            }
            using (SqlConnection vcr = new SqlConnection(connectionString))
            {
                vcr.Open();
                using (SqlTransaction wer = vcr.BeginTransaction())
                {
                    try
                    {
                        foreach (DataTable dt in ds.Tables)
                        {
                            if (dt == null || dt.Rows.Count < 1 || string.IsNullOrEmpty(dt.TableName)) continue;
                            using (SqlBulkCopy a = new SqlBulkCopy(vcr, SqlBulkCopyOptions.Default, wer)
                                )
                            {
                                a.BulkCopyTimeout = 60;
                                a.DestinationTableName = dt.TableName;
                                foreach (DataColumn col in dt.Columns)
                                {
                                    a.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                                }
                                a.WriteToServer(dt);
                            }
                        }
                        wer.Commit();
                    }
                    catch (Exception)
                    {
                        wer.Rollback();
                        throw;
                    }
                    finally
                    {
                        vcr.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 批量插入数据
        /// </summary>
        /// <param name="timeOut">超时时间，单位秒，默认60s</param>
        public static void BulkCopy(IList<DataTable> ds, string connectionString = null, int timeOut = 60)
        {
            if (ds == null || !ds.Any()) return;
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = _s;
            }
            using (SqlConnection a = new SqlConnection(connectionString))
            {
                a.Open();
                using (SqlTransaction b = a.BeginTransaction())
                {
                    try
                    {
                        foreach (DataTable k in ds)
                        {
                            if (k == null || k.Rows.Count < 1 || string.IsNullOrEmpty(k.TableName)) continue;
                            using (SqlBulkCopy w = new SqlBulkCopy(a, SqlBulkCopyOptions.Default, b)
                                )
                            {
                                w.BulkCopyTimeout = 60;
                                w.DestinationTableName = k.TableName;
                                foreach (DataColumn col in k.Columns)
                                {
                                    w.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                                }
                                w.WriteToServer(k);
                            }
                        }
                        b.Commit();
                    }
                    catch (Exception)
                    {
                        b.Rollback();
                        throw;
                    }
                    finally
                    {
                        a.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 批量插入数据
        /// </summary>
        /// <param name="timeOut">超时时间，单位秒，默认60s</param>
        public static void BulkCopy(ConcurrentBag<DataTable> ds, string connectionString = null, int timeOut = 60)
        {
            if (ds == null || !ds.Any()) return;
            Parallel.ForEach(ds, dt => BulkCopy(dt, connectionString, timeOut));
        }

        private static string GetDeleteOrDropDataTableSqlByName(List<string> tableNameList, string dataBase, bool isView, int type, string connectionString = null)
        {
            if (tableNameList == null || tableNameList.Count < 1) return string.Empty;
            var a = new Dictionary<string, int>();
            var b = new Dictionary<string, int>();
            StringBuilder c = new StringBuilder();
            var d = type == 1 ? " DELETE FROM " : (type == 0 ? " DROP TABLE " : string.Empty);
            foreach (var e in tableNameList.Distinct().Where(tableName => !string.IsNullOrEmpty(tableName)))
            {
                if (b.ContainsKey(e)) continue;
                var f = GetDeleteTableNameList(e, b, connectionString);
                if (!string.IsNullOrEmpty(dataBase))
                {
                    dataBase = dataBase + ".dbo.";
                }
                string s = ";IF EXISTS ( SELECT * FROM " + dataBase + " sysobjects WHERE name = '{0}' AND type = '" + (isView ? "V" : "U") + "') " + d + dataBase + "[{0}] ;";
                if (f == null || f.Count < 1) continue;
                foreach (var tempString in f)
                {
                    if (a.ContainsKey(tempString))
                    {
                        a[tempString]++;
                    }
                    else
                    {
                        a.Add(tempString, 1);
                        c.AppendFormat(s, tempString);
                    }
                }
            }
            return c.ToString();
        }

        /// <summary>
        /// 根据表或视图名删除一个表或视图数据
        /// </summary>
        /// <author>FreshMan</author>
        /// <creattime>2017-09-5</creattime>
        /// <returns>删除字符串</returns>
        public static string GetDeleteDataTableSqlByName(string tableName, string dataBase = null, bool isView = false, string connectionString = null)
        {
            return GetDeleteOrDropDataTableSqlByName(new List<string> { tableName }, dataBase, isView, 1, connectionString);
        }

        /// <summary>
        /// 根据表或视图名删除一个表或视图数据
        /// </summary>
        /// <author>FreshMan</author>
        /// <creattime>2017-09-06</creattime>
        /// <returns>删除字符串</returns>
        public static string GetDeleteDataTableSqlByName(List<string> tableNameList, string dataBase = null, bool isView = false, string connectionString = null)
        {
            return GetDeleteOrDropDataTableSqlByName(tableNameList, dataBase, isView, 1, connectionString);
        }

        /// <summary>
        /// 根据表或视图名删除一个表或视图结构
        /// </summary>
        /// <author>FreshMan</author>
        /// <creattime>2017-09-06</creattime>
        /// <returns>删除字符串</returns>
        public static string GetDropDataTableSqlByName(string tableName, string dataBase = null, bool isView = false, string connectionString = null)
        {
            return GetDeleteOrDropDataTableSqlByName(new List<string> { tableName }, dataBase, isView, 1, connectionString);
        }

        /// <summary>
        /// 根据表或视图名删除一个表或视图结构
        /// </summary>
        /// <author>FreshMan</author>
        /// <creattime>2017-09-06</creattime>
        /// <returns>删除字符串</returns>
        public static string GetDropDataTableSqlByName(List<string> tableNameList, string dataBase = null, bool isView = false, string connectionString = null)
        {
            return GetDeleteOrDropDataTableSqlByName(tableNameList, dataBase, isView, 0, connectionString);
        }

        private static Dictionary<string, List<string>> GetReferencedMap(string connectionString = null)
        {
            Dictionary<string, List<string>> b = new Dictionary<string, List<string>>();
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
            var r = new List<ReferencedModel>();
            var v = GetReader(sqlCmd, connectionString);
            while (v.Read())
            {
                var a = new ReferencedModel
                {
                    ForeignKey = (string)v["ForeignKey"],
                    ForeignKeyCell = (string)v["ForeignKeyCell"],
                    ReferencedCell = (string)v["ReferencedCell"],
                    ReferencedTableName = (string)v["ReferencedTableName"],
                    TableName = (string)v["TableName"]
                };
                r.Add(a);
                if (b.ContainsKey(a.ReferencedTableName)) continue;
                b.Add(a.ReferencedTableName, new List<string>());
            }
            v.Close();
            foreach (var rowModel in r.Where(rowModel => rowModel.ReferencedTableName != rowModel.TableName))
            {
                b[rowModel.ReferencedTableName].Add(rowModel.TableName);
            }
            return b;
        }

        private static List<string> GetDeleteTableNameList(string tableName, Dictionary<string, int> historyDictionary, string connectionString = null)
        {
            if (string.IsNullOrEmpty(tableName)) return null;
            var r = GetReferencedMap(connectionString);
            if (!r.ContainsKey(tableName))
            {
                historyDictionary.Add(tableName, 1);
                return new List<string> { tableName };
            }
            var n = new HashSet<string>();
            var a = TraversingGraph(r, tableName, historyDictionary, n);
            return a;
        }

        private static List<string> TraversingGraph(Dictionary<string, List<string>> sourceDictionary, string nodeName, Dictionary<string, int> historyDictionary, HashSet<string> nodeSet)
        {
            var a = new List<string>();
            if (!nodeSet.Add(nodeName)) return a;
            if (historyDictionary.ContainsKey(nodeName)) return a;
            if (!sourceDictionary.ContainsKey(nodeName))
            {
                historyDictionary.Add(nodeName, 1);
                a.Add(nodeName);
                return a;
            }
            for (int i = 0; i < sourceDictionary[nodeName].Count; i++)
            {
                var b = sourceDictionary[nodeName][i];
                var c = TraversingGraph(sourceDictionary, b, historyDictionary, nodeSet);
                if (c != null && c.Any()) a.AddRange(c);
            }
            a.Add(nodeName);
            historyDictionary.Add(nodeName, 1);
            return a;
        }

        /// <summary>
        /// 获取单条记录
        /// </summary>
        /// <returns></returns>
        public static T Get<T>(object searchModel) where T : class, new()
        {
            var w = typeof(T);
            var h = GetIdProperties(w).ToList();
            if (!h.Any())
                throw new ArgumentException("Get<T> only supports an entity with a [Key] or Id property");
            var a = GetTableName(w);
            var t = new StringBuilder();
            t.Append("select ");
            BuildSelect(t, GetScaffoldableProperties(w));
            t.AppendFormat(" from {0} where ", a);

            for (int i = 0; i < h.Count; i++)
            {
                if (i > 0)
                {
                    t.Append(" and ");
                }
                t.AppendFormat("{0}=@{1}", GetColumnName(h[i]), h[i].Name);
            }
            List<SqlParameter> f = new List<SqlParameter>();
            if (h.Count == 1)
            {
                var u = h.First();
                var c = searchModel.GetType().GetProperty(u.Name);
                if (c != null)
                    f.Add(new SqlParameter
                    {
                        ParameterName = "@" + u.Name,
                        Value = c.GetValue(searchModel, null)
                    });
            }
            else
            {
                f.AddRange(from prop in h
                           let propertyInfo = searchModel.GetType().GetProperty(prop.Name)
                           where propertyInfo != null
                           select new SqlParameter
                           {
                               ParameterName = "@" + prop.Name,
                               Value = propertyInfo.GetValue(searchModel, null)
                           });
            }
            var dt = GetDataTable(t.ToString(), f.ToArray());
            var k = DataTypeConvertHelper.ToList<T>(dt);
            return k?.FirstOrDefault();
        }

        /// <summary>
        /// 获取记录集合
        /// </summary>
        /// <returns></returns>
        public static List<T> Get<T>(string sql, string connectionString = null) where T : new()
        {
            if (string.IsNullOrEmpty(sql)) return null;
            var a = GetReader(sql, connectionString);
            var b = new List<T>();
            if (a != null)
            {
                while (a.Read())
                {
                    T t = new T();
                    t = EntityUtilCache<T>.SetPropertyInvoker(t, a);
                    b.Add(t);
                }
                a.Close();
            }
            return b;
        }

        /// <summary>
        /// 获取记录集合
        /// </summary>
        /// <returns></returns>
        public static List<T> Get<T>(string sql, SqlParameter[] values, string connectionString = null) where T : new()
        {
            if (string.IsNullOrEmpty(sql)) return null;
            var c = GetReader(sql, values, connectionString);
            var d = new List<T>();
            if (c != null)
            {
                while (c.Read())
                {
                    T t = new T();
                    t = EntityUtilCache<T>.SetPropertyInvoker(t, c);
                    d.Add(t);
                }
                c.Close();
            }
            return d;
        }

        private static void BuildSelect(StringBuilder sb, IEnumerable<PropertyInfo> props)
        {
            var p = props as IList<PropertyInfo> ?? props.ToList();
            if (!p.Any()) return;
            var s = false;
            for (int i = 0; i < p.Count(); i++)
            {
                if (!p.ElementAt(i).CanWrite) continue;
                if (p.ElementAt(i).GetCustomAttributes(true).Any(attr => attr.GetType().Name == typeof(NonPersistentAttribute).Name)) { continue; }
                if (s)
                {
                    sb.Append(",");
                }
                sb.Append(GetColumnName(p.ElementAt(i)));
                if (p.ElementAt(i).GetCustomAttributes(true).SingleOrDefault(attr => attr.GetType().Name == typeof(ColumnAttribute).Name) != null)
                    sb.Append(" as " + Encapsulate(p.ElementAt(i).Name));
                s = true;
            }
        }
    }
}
