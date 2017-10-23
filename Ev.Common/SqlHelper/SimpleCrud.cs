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
        #region [1、全局变量]

        /// <summary>
        /// 链接字符串
        /// </summary>
        private static string _connectionString;

        /// <summary>
        /// 初始化函数
        /// </summary>
        static SimpleCrud()
        {
            _connectionString = string.IsNullOrEmpty(_connectionString)
                    ? ConfigurationManager.AppSettings["ConstrSQL"]
                    : _connectionString;
        }

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
        #endregion

        #region [2、Execute执行方法]

        /// <summary>
        /// 执行sql
        /// </summary>
        /// <param name="safeSql">SQL命令</param>
        /// <param name="connectionString">链接字符串，为null,使用全局配置项</param>
        /// <returns>执行结果影响行数</returns>
        public static int ExecuteCommand(string safeSql, string connectionString = null)
        {
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = _connectionString;
                }
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(safeSql, conn);
                    int result = cmd.ExecuteNonQuery();
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
        /// 带参数的执行命令
        /// </summary>
        /// <param name="sql">SQL命令</param>
        /// <param name="values">参数化参数</param>
        /// <param name="connectionString">链接字符串，为null,使用全局配置项</param>
        /// <returns>执行结果影响行数</returns>
        public static int ExecuteCommand(string sql, SqlParameter[] values, string connectionString = null)
        {
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = _connectionString;
                }
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand(sql, connection);
                    if (values.Length > 0)
                    {
                        cmd.Parameters.AddRange(values);
                    }
                    return cmd.ExecuteNonQuery();
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
        /// 执行查询
        /// </summary>
        /// <param name="sql">SQL命令</param>
        /// <param name="values">参数化参数</param>
        /// <param name="transaction">是否事务执行：true:事务执行（sql语句为单条事务语句），false:非事务执行</param>
        /// <param name="timeOut">命令超时时间 （以秒为单位）默认值为 30 秒</param>
        /// <param name="connectionString">链接字符串，为null,使用全局配置项</param>
        /// <returns>执行结果影响行数</returns>
        public static int ExecuteCommand(string sql, IEnumerable<SqlParameter> values, bool transaction = false, int? timeOut = null, string connectionString = null)
        {

            SqlTransaction tranProducts = null;
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = _connectionString;
                }
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand(sql, connection);
                    if (transaction)
                    {
                        tranProducts = connection.BeginTransaction();
                        cmd.Transaction = tranProducts;
                    }
                    if (timeOut != null && timeOut > 0)
                    {
                        cmd.CommandTimeout = (int)timeOut;
                    }
                    var paramtersList = values.ToArray();
                    if (paramtersList.Length > 0)
                    {
                        cmd.Parameters.AddRange(paramtersList);
                    }
                    var result = cmd.ExecuteNonQuery();
                    if (transaction)
                    {
                        tranProducts.Commit();
                    }
                    return result;
                }

            }
            catch (Exception ex)
            {
                if (transaction)
                {
                    try
                    {
                        tranProducts?.Rollback();
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
        /// <param name="sql">SQL命令</param>
        /// <param name="values">参数化参数</param>
        /// <param name="transaction">是否事务执行：true:事务执行（sql语句为单条事务语句），false:非事务执行</param>
        /// <param name="timeOut">命令超时时间 （以秒为单位）默认值为 30 秒</param>
        /// <param name="connectionString">链接字符串，为null,使用全局配置项</param>
        /// <returns>执行结果影响行数</returns>
        public static bool ExecuteCommand(List<string> sql, IEnumerable<SqlParameter> values, bool transaction = false, int? timeOut = null, string connectionString = null)
        {

            SqlTransaction tranProducts = null;
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = _connectionString;
                }
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand { Connection = connection };
                    if (transaction)
                    {
                        tranProducts = connection.BeginTransaction();
                        cmd.Transaction = tranProducts;
                    }
                    foreach (var tempSql in sql)
                    {
                        if (transaction)
                        {
                            cmd.Transaction = tranProducts;
                        }
                        cmd.CommandText = tempSql;
                        cmd.ExecuteNonQuery();
                    }
                    if (timeOut != null && timeOut > 0)
                    {
                        cmd.CommandTimeout = (int)timeOut;
                    }
                    var paramtersList = values.ToArray();
                    if (paramtersList.Length > 0)
                    {
                        cmd.Parameters.AddRange(paramtersList);
                    }
                    if (transaction)
                    {
                        tranProducts.Commit();
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
                        tranProducts?.Rollback();
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
        #endregion

        #region [3、Scalar执行方法]

        /// <summary>
        /// 执行只返回单行单列的值
        /// </summary>
        /// <param name="safeSql">SQL命令</param>
        /// <param name="connectionString">链接字符串，为null,使用全局配置项</param>
        /// <returns>执行只返回单行单列的值</returns>
        public static object GetScalar(string safeSql, string connectionString = null)
        {
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = _connectionString;
                }
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand(safeSql, connection);
                    var result = cmd.ExecuteScalar();
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
        /// <param name="sql">SQL命令</param>
        /// <param name="values">参数化参数</param>
        /// <param name="connectionString">链接字符串，为null,使用全局配置项</param>
        /// <returns>执行只返回单行单列的值</returns>
        public static object GetScalar(string sql, SqlParameter[] values, string connectionString = null)
        {
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = _connectionString;
                }
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand(sql, connection);
                    if (values.Length > 0)
                    {
                        cmd.Parameters.AddRange(values);
                    }
                    var result = cmd.ExecuteScalar();
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
        /// <param name="sql">SQL命令</param>
        /// <param name="values">参数化参数</param>
        /// <param name="transaction">是否事务</param>
        /// <param name="timeOut">命令超时时间（以秒为单位）默认值为 30 秒</param>
        /// <param name="connectionString">链接字符串，为null,使用全局配置项</param>
        /// <returns></returns>
        public static dynamic GetScalar(string sql, IEnumerable<SqlParameter> values, bool transaction = false, int? timeOut = null, string connectionString = null)
        {
            SqlTransaction tranProducts = null;
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = _connectionString;
                }
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand(sql, connection);
                    if (transaction)
                    {
                        tranProducts = connection.BeginTransaction();
                    }
                    if (timeOut != null && timeOut > 0)
                    {
                        cmd.CommandTimeout = (int)timeOut;
                    }
                    var paramtersList = values.ToArray();
                    if (paramtersList.Length > 0)
                    {
                        cmd.Parameters.AddRange(paramtersList);
                    }
                    cmd.Transaction = tranProducts;
                    var result = cmd.ExecuteScalar();
                    if (transaction)
                    {
                        tranProducts.Commit();
                    }
                    return result;
                }
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

        #region [4、Reader执行方法]
        /// <summary>
        /// 获取一个读取器
        /// </summary>
        /// <param name="safeSql">SQL命令</param>
        /// <param name="connectionString">链接字符串，为null,使用全局配置项</param>
        /// <returns>获取一个读取器</returns>
        public static SqlDataReader GetReader(string safeSql, string connectionString = null)
        {
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = _connectionString;
                }
                var connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand cmd = new SqlCommand(safeSql, connection);
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
        /// <param name="sql">SQL命令</param>
        /// <param name="values">参数化参数</param>
        /// <param name="connectionString">链接字符串，为null,使用全局配置项</param>
        /// <returns>获取一个读取器</returns>
        public static SqlDataReader GetReader(string sql, SqlParameter[] values, string connectionString = null)
        {
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = _connectionString;
                }
                var connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand cmd = new SqlCommand(sql, connection);
                if (values.Length > 0)
                {
                    cmd.Parameters.AddRange(values);
                }
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
        #endregion

        #region [5、执行查询操作DataTable DataSet]
        /// <summary>
        /// 无参数sql返回DataTable
        /// </summary>
        /// <param name="safeSql">SQL命令</param>
        /// <param name="connectionString">链接字符串，为null,使用全局配置项</param>
        /// <returns>结果集合</returns>
        public static DataTable GetDataTable(string safeSql, string connectionString = null)
        {
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = _connectionString;
                }
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    DataSet ds = new DataSet();
                    SqlCommand cmd = new SqlCommand(safeSql, connection);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(ds);
                    return ds.Tables[0];
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
        /// <param name="sql">SQL命令</param>
        /// <param name="values">参数化参数</param>
        /// <param name="connectionString">链接字符串，为null,使用全局配置项</param>
        /// <returns>结果集合</returns>
        public static DataTable GetDataTable(string sql, SqlParameter[] values, string connectionString = null)
        {
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = _connectionString;
                }
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    DataSet ds = new DataSet();
                    SqlCommand cmd = new SqlCommand(sql, connection);
                    if (values.Length > 0)
                    {
                        cmd.Parameters.AddRange(values);
                    }
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(ds);
                    return ds.Tables[0];
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
        /// <param name="sql">SQL命令</param>
        /// <param name="table">表名称</param>
        /// <param name="connectionString">链接字符串，为null,使用全局配置项</param>
        /// <returns>结果集合</returns>
        public static DataSet GetDataSet(string sql, string table, string connectionString = null)
        {
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = _connectionString;
                }
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    DataSet ds = new DataSet();
                    SqlCommand cmd = new SqlCommand(sql, connection);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(ds, table);
                    return ds;
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
        /// <param name="sql">SQL命令</param>
        /// <param name="table">表名称</param>
        /// <param name="values">参数化参数</param>
        /// <param name="connectionString">链接字符串，为null,使用全局配置项</param>
        /// <returns></returns>
        public static DataSet GetDataSet(string sql, string table, SqlParameter[] values, string connectionString = null)
        {
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = _connectionString;
                }
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    DataSet ds = new DataSet();
                    SqlCommand cmd = new SqlCommand(sql, connection);
                    if (values.Length > 0)
                    {
                        cmd.Parameters.AddRange(values);
                    }
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(ds, table);
                    return ds;
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
        #endregion

        #region [6、批量操作]
        /// <summary> 
        /// 批量更新数据
        /// </summary>  
        /// <param name="table"></param> 
        /// <param name="connectionString">链接字符串，为null,使用全局配置项</param>
        public static void BulkUpdate(DataTable table, string connectionString = null)
        {
            if (table == null || table.Rows.Count < 1) return;
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = _connectionString;
            }
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand comm = conn.CreateCommand();
                comm.CommandTimeout = 30;
                comm.CommandType = CommandType.Text;
                SqlDataAdapter adapter = new SqlDataAdapter(comm);
                SqlCommandBuilder commandBulider = new SqlCommandBuilder(adapter);
                commandBulider.ConflictOption = ConflictOption.OverwriteChanges;
                try
                {
                    conn.Open();
                    adapter.SelectCommand.Transaction = conn.BeginTransaction();
                    adapter.Update(table);
                    adapter.SelectCommand.Transaction.Commit();
                }
                catch (Exception)
                {
                    adapter.SelectCommand?.Transaction?.Rollback();
                    throw;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        /// <summary> 
        /// 大批量插入数据 
        /// 已采用整体事物控制 
        /// </summary> 
        /// <param name="tableName">数据库服务器上目标表名</param> 
        /// <param name="dt">含有和目标数据库表结构完全一致(所包含的字段名完全一致即可)的DataTable</param> 
        /// <param name="connectionString">链接字符串，为null,使用全局配置项</param>
        /// <param name="timeOut">超时时间，单位秒，默认60s</param>
        public static void BulkCopy(string tableName, DataTable dt, string connectionString = null, int timeOut = 60)
        {
            if (string.IsNullOrEmpty(tableName) || dt == null || dt.Rows.Count < 0) return;
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = _connectionString;
            }
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, transaction))
                    {
                        bulkCopy.BulkCopyTimeout = timeOut;
                        bulkCopy.DestinationTableName = tableName;
                        try
                        {
                            foreach (DataColumn col in dt.Columns)
                            {
                                bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                            }
                            bulkCopy.WriteToServer(dt);
                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 大批量插入数据
        /// </summary>
        /// <param name="dt">含有和目标数据库表结构完全一致(所包含的字段名完全一致即可)的DataTable</param>
        /// <param name="connectionString">链接字符串，为null,使用全局配置项</param>
        /// <param name="timeOut">超时时间，单位秒，默认60s</param>
        public static void BulkCopy(DataTable dt, string connectionString = null, int timeOut = 60)
        {
            BulkCopy(dt.TableName, dt, connectionString, timeOut);
        }

        /// <summary>
        /// 批量插入数据
        /// </summary>
        /// <param name="ds">多个Table集合，每个Table中含有和目标数据库表结构完全一致(所包含的字段名完全一致即可)的DataTable，Table名称作为表名称</param>
        /// <param name="connectionString">链接字符串，为null,使用全局配置项</param>
        /// <param name="timeOut">超时时间，单位秒，默认60s</param>
        public static void BulkCopy(DataSet ds, string connectionString = null, int timeOut = 60)
        {
            if (ds == null || ds.Tables.Count < 1) return;
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = _connectionString;
            }
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (DataTable dt in ds.Tables)
                        {
                            if (dt == null || dt.Rows.Count < 1 || string.IsNullOrEmpty(dt.TableName)) continue;
                            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, transaction)
                                )
                            {
                                //bulkCopy.BatchSize = 20000;
                                bulkCopy.BulkCopyTimeout = timeOut;
                                bulkCopy.DestinationTableName = dt.TableName;
                                foreach (DataColumn col in dt.Columns)
                                {
                                    bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                                }
                                bulkCopy.WriteToServer(dt);
                            }
                        }
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 批量插入数据
        /// </summary>
        /// <param name="ds">多个Table集合，每个Table中含有和目标数据库表结构完全一致(所包含的字段名完全一致即可)的DataTable，Table名称作为表名称</param>
        /// <param name="connectionString">链接字符串，为null,使用全局配置项</param>
        /// <param name="timeOut">超时时间，单位秒，默认60s</param>
        public static void BulkCopy(IList<DataTable> ds, string connectionString = null, int timeOut = 60)
        {
            if (ds == null || !ds.Any()) return;
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = _connectionString;
            }
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (DataTable dt in ds)
                        {
                            if (dt == null || dt.Rows.Count < 1 || string.IsNullOrEmpty(dt.TableName)) continue;
                            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, transaction)
                                )
                            {
                                //bulkCopy.BatchSize = 20000;
                                bulkCopy.BulkCopyTimeout = timeOut;
                                bulkCopy.DestinationTableName = dt.TableName;
                                foreach (DataColumn col in dt.Columns)
                                {
                                    bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                                }
                                bulkCopy.WriteToServer(dt);
                            }
                        }
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 批量插入数据
        /// </summary>
        /// <param name="ds">多个Table集合，每个Table中含有和目标数据库表结构完全一致(所包含的字段名完全一致即可)的DataTable，Table名称作为表名称</param>
        /// <param name="connectionString">链接字符串，为null,使用全局配置项</param>
        /// <param name="timeOut">超时时间，单位秒，默认60s</param>
        public static void BulkCopy(ConcurrentBag<DataTable> ds, string connectionString = null, int timeOut = 60)
        {
            if (ds == null || !ds.Any()) return;
            Parallel.ForEach(ds, dt => BulkCopy(dt, connectionString, timeOut));
        }
        #endregion

        #region [7、删除Table,View数据SQL Code]

        /// <summary>
        /// 根据表或视图名删除一个表或视图数据
        /// </summary>
        /// <param name="tableNameList">表或视图名称集合</param>
        /// <param name="dataBase">数据库名称，默认为当前链接数据库</param>
        /// <param name="isView">是否是视图：true：是；false：否（默认）</param>
        /// <param name="type">1:delete;0:drop</param>
        /// <param name="connectionString">链接字符串，为null,使用全局配置项</param>
        /// <author>FreshMan</author>
        /// <creattime>2017-09-06</creattime>
        /// <returns>删除字符串</returns>
        private static string GetDeleteOrDropDataTableSqlByName(List<string> tableNameList, string dataBase, bool isView, int type, string connectionString = null)
        {
            if (tableNameList == null || tableNameList.Count < 1) return string.Empty;
            var hadDeleteTable = new Dictionary<string, int>();
            var historyDictionary = new Dictionary<string, int>();
            StringBuilder resulteBuilder = new StringBuilder();
            var typeSql = type == 1 ? " DELETE FROM " : (type == 0 ? " DROP TABLE " : string.Empty);
            foreach (var tableName in tableNameList.Distinct().Where(tableName => !string.IsNullOrEmpty(tableName)))
            {
                if (historyDictionary.ContainsKey(tableName)) continue;
                var referencedTableList = GetDeleteTableNameList(tableName, historyDictionary, connectionString);
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
        /// <param name="connectionString">链接字符串，为null,使用全局配置项</param>
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
        /// <param name="tableNameList">表或视图名称集合</param>
        /// <param name="dataBase">数据库名称，默认为当前链接数据库</param>
        /// <param name="isView">是否是视图：true：是；false：否（默认）</param>
        /// <param name="connectionString">链接字符串，为null,使用全局配置项</param>
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
        /// <param name="tableName">表或视图名称集合</param>
        /// <param name="dataBase">数据库名称，默认为当前链接数据库</param>
        /// <param name="isView">是否是视图：true：是；false：否（默认）</param>
        /// <param name="connectionString">链接字符串，为null,使用全局配置项</param>
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
        /// <param name="tableNameList">表或视图名称集合</param>
        /// <param name="dataBase">数据库名称，默认为当前链接数据库</param>
        /// <param name="isView">是否是视图：true：是；false：否（默认）</param>
        /// <param name="connectionString">链接字符串，为null,使用全局配置项</param>
        /// <author>FreshMan</author>
        /// <creattime>2017-09-06</creattime>
        /// <returns>删除字符串</returns>
        public static string GetDropDataTableSqlByName(List<string> tableNameList, string dataBase = null, bool isView = false, string connectionString = null)
        {
            return GetDeleteOrDropDataTableSqlByName(tableNameList, dataBase, isView, 0, connectionString);
        }

        /// <summary>
        /// 获取引用图
        /// <param name="connectionString">链接字符串，为null,使用全局配置项</param>
        /// <author>FreshMan</author>
        /// <creattime>2017-09-06</creattime>
        /// </summary>
        private static Dictionary<string, List<string>> GetReferencedMap(string connectionString = null)
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
            var dr = GetReader(sqlCmd, connectionString);
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
        /// <param name="connectionString">链接字符串，为null,使用全局配置项</param>
        /// <author>FreshMan</author>
        /// <creattime>2017-09-06</creattime>
        /// <returns></returns>
        private static List<string> GetDeleteTableNameList(string tableName, Dictionary<string, int> historyDictionary, string connectionString = null)
        {
            if (string.IsNullOrEmpty(tableName)) return null;
            var referencedMap = GetReferencedMap(connectionString);
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

        #region [8、获得数据]

        /// <summary>
        /// 获取单条记录
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="searchModel">主键Id</param>
        /// <returns></returns>
        public static T Get<T>(object searchModel) where T : class, new()
        {
            var currenttype = typeof(T);
            var idProps = GetIdProperties(currenttype).ToList();
            if (!idProps.Any())
                throw new ArgumentException("Get<T> only supports an entity with a [Key] or Id property");
            var name = GetTableName(currenttype);
            var sb = new StringBuilder();
            sb.Append("select ");
            BuildSelect(sb, GetScaffoldableProperties(currenttype));
            sb.AppendFormat(" from {0} where ", name);

            for (int i = 0; i < idProps.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(" and ");
                }
                sb.AppendFormat("{0}=@{1}", GetColumnName(idProps[i]), idProps[i].Name);
            }
            List<SqlParameter> dynParms = new List<SqlParameter>();
            if (idProps.Count == 1)
            {
                var prop = idProps.First();
                var propertyInfo = searchModel.GetType().GetProperty(prop.Name);
                if (propertyInfo != null)
                    dynParms.Add(new SqlParameter
                    {
                        ParameterName = "@" + prop.Name,
                        Value = propertyInfo.GetValue(searchModel, null)
                    });
            }
            else
            {
                dynParms.AddRange(from prop in idProps
                                  let propertyInfo = searchModel.GetType().GetProperty(prop.Name)
                                  where propertyInfo != null
                                  select new SqlParameter
                                  {
                                      ParameterName = "@" + prop.Name,
                                      Value = propertyInfo.GetValue(searchModel, null)
                                  });
            }
            var dt = GetDataTable(sb.ToString(), dynParms.ToArray());
            var resultModel = DataTypeConvertHelper.ToList<T>(dt);
            return resultModel?.FirstOrDefault();
        }

        /// <summary>
        /// 获取记录集合
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="sql">查询sql</param>
        /// <param name="connectionString">链接字符串，为null,使用全局配置项</param>
        /// <returns></returns>
        public static List<T> Get<T>(string sql, string connectionString = null) where T : new()
        {
            if (string.IsNullOrEmpty(sql)) return null;
            var reader = GetReader(sql, connectionString);
            var listEntity = new List<T>();
            if (reader != null)
            {
                while (reader.Read())
                {
                    T t = new T();
                    t = EntityUtilCache<T>.SetPropertyInvoker(t, reader);
                    listEntity.Add(t);
                }
                reader.Close();
            }
            return listEntity;
        }

        /// <summary>
        /// 获取记录集合
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="sql">查询sql</param>
        /// <param name="values">参数化值</param>
        /// <param name="connectionString">链接字符串，为null,使用全局配置项</param>
        /// <returns></returns>
        public static List<T> Get<T>(string sql, SqlParameter[] values, string connectionString = null) where T : new()
        {
            if (string.IsNullOrEmpty(sql)) return null;
            var reader = GetReader(sql, values, connectionString);
            var listEntity = new List<T>();
            if (reader != null)
            {
                while (reader.Read())
                {
                    T t = new T();
                    t = EntityUtilCache<T>.SetPropertyInvoker(t, reader);
                    listEntity.Add(t);
                }
                reader.Close();
            }
            return listEntity;
        }

        /// <summary>
        /// 获得查询结果
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="props"></param>
        private static void BuildSelect(StringBuilder sb, IEnumerable<PropertyInfo> props)
        {
            var propertyInfos = props as IList<PropertyInfo> ?? props.ToList();
            if (!propertyInfos.Any()) return;
            var addedAny = false;
            for (int i = 0; i < propertyInfos.Count(); i++)
            {
                if (!propertyInfos.ElementAt(i).CanWrite) continue;
                if (propertyInfos.ElementAt(i).GetCustomAttributes(true).Any(attr => attr.GetType().Name == typeof(NonPersistentAttribute).Name)) { continue; }
                if (addedAny)
                {
                    sb.Append(",");
                }
                sb.Append(GetColumnName(propertyInfos.ElementAt(i)));
                if (propertyInfos.ElementAt(i).GetCustomAttributes(true).SingleOrDefault(attr => attr.GetType().Name == typeof(ColumnAttribute).Name) != null)
                    sb.Append(" as " + Encapsulate(propertyInfos.ElementAt(i).Name));
                addedAny = true;
            }
        }
        #endregion
    }
}
