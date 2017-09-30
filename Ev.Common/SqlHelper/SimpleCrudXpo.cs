/**============================================================
* 命名空间: Ev.Common.SqlHelper
*
* 功 能： N/A
* 类 名： SimpleCrudXpo
*
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
* V0.01 2017/9/18 11:49:54 FreshMan 初版
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
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using DevExpress.Xpo;
using Ev.Common.DataConvert;
using Microsoft.CSharp.RuntimeBinder;

namespace Ev.Common.SqlHelper
{
    /// <summary>
    /// 创建关于XPO相关操作的SQL帮助
    /// </summary>
    public static partial class SimpleCrud
    {
        #region [1、局部变量]

        /// <summary>
        /// 数据库前缀
        /// </summary>
        private static string _encapsulation = "[{0}]";

        /// <summary>
        /// 数据库类型
        /// </summary>
        private static string _dialect = "SQLServer";

        /// <summary>
        /// 获取自增长的id值
        /// </summary>
        private static string _getIdentitySql = "SELECT CAST(SCOPE_IDENTITY()  AS BIGINT) AS [id]";

        /// <summary>
        /// 排除的字段名称
        /// </summary>
        private static readonly List<string> ExcludeFieldList = new List<string> { "Loading", "IsLoading", "IsDeleted" };

        /// <summary>
        /// 表缓存
        /// </summary>
        private static readonly ConcurrentDictionary<Type, Tuple<string, string>> TableNames = new ConcurrentDictionary<Type, Tuple<string, string>>();

        /// <summary>
        /// 列缓存
        /// </summary>
        private static readonly ConcurrentDictionary<string, Tuple<string, string>> ColumnNames = new ConcurrentDictionary<string, Tuple<string, string>>();

        /// <summary>
        /// 类型中的字段缓存
        /// </summary>
        private static readonly ConcurrentDictionary<string, List<PropertyInfo>> TypePropertyCaches = new ConcurrentDictionary<string, List<PropertyInfo>>();

        /// <summary>
        /// 表名称转化器
        /// </summary>
        private static readonly ITableNameResolver SelftableNameResolver = new TableNameResolver();

        /// <summary>
        /// 列名转化器
        /// </summary>
        private static readonly IColumnNameResolver SelfcolumnNameResolver = new ColumnNameResolver();
        #endregion

        #region [2、创建插入SQL]

        /// <summary>
        /// 创建插入SQL
        /// </summary>
        /// <param name="entityToInsert"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <typeparam name="TKey">仅仅支持一下类型：int,uint,long,ulong,short,ushort,Guid,string</typeparam>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public static TKey Insert<TKey, TEntity>(TEntity entityToInsert,
            bool transaction = false, int? commandTimeout = null)
        {
            var type = entityToInsert.GetType();
            var idProps = GetIdProperties(entityToInsert).ToList();

            if (!idProps.Any())
                throw new ArgumentException("Insert<T> only supports an entity with a [Key] or Id property");
            var keyHasPredefinedValue = false;
            var baseType = typeof(TKey);
            var underlyingType = Nullable.GetUnderlyingType(baseType);
            var keytype = underlyingType ?? baseType;
            if (keytype != typeof(int) && keytype != typeof(uint) && keytype != typeof(long) && keytype != typeof(ulong) && keytype != typeof(short) && keytype != typeof(ushort) && keytype != typeof(Guid) && keytype != typeof(string))
            {
                throw new Exception("Invalid return type");
            }
            var name = GetTableName(entityToInsert);
            var sb = new StringBuilder();
            sb.AppendFormat(" insert into {0} ", name);
            sb.Append(" (");
            BuildInsertParameters(sb, type);
            sb.Append(") ");
            sb.Append("values");
            sb.Append(" (");
            BuildInsertValues(sb, type);
            sb.Append(")");
            if (keytype == typeof(Guid))
            {
                var guidvalue = (Guid)idProps.First().GetValue(entityToInsert, null);
                if (guidvalue == Guid.Empty)
                {
                    var newguid = SequentialGuid();
                    idProps.First().SetValue(entityToInsert, newguid, null);
                }
                else
                {
                    keyHasPredefinedValue = true;
                }
                sb.Append(";select '" + idProps.First().GetValue(entityToInsert, null) + "' as id");
            }
            if ((keytype == typeof(int) || keytype == typeof(long)) &&
                Convert.ToInt64(idProps.First().GetValue(entityToInsert, null)) == 0)
            {
                sb.Append(";" + _getIdentitySql);
            }
            else
            {
                keyHasPredefinedValue = true;
            }
            var paramters = ObjectToParameters(entityToInsert);
            var r = Query(sb.ToString(), paramters, transaction, commandTimeout);
            if (keytype == typeof(Guid) || keyHasPredefinedValue)
            {
                return (TKey)idProps.First().GetValue(entityToInsert, null);
            }
            if (r == null)
            {
                return default(TKey);
            }
            return (TKey)r;
        }


        /// <summary>
        /// 创建插入SQL
        /// </summary>
        /// <param name="entityToInsert"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <typeparam name="TKey">仅仅支持一下类型：int,uint,long,ulong,short,ushort,Guid,string</typeparam>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public static string GetInsertSql<TKey, TEntity>(TEntity entityToInsert,
            bool transaction = false, int? commandTimeout = null)
        {
            var type = entityToInsert.GetType();
            var idProps = GetIdProperties(entityToInsert).ToList();

            if (!idProps.Any())
                throw new ArgumentException("Insert<T> only supports an entity with a [Key] or Id property");
            var baseType = typeof(TKey);
            var underlyingType = Nullable.GetUnderlyingType(baseType);
            var keytype = underlyingType ?? baseType;
            if (keytype != typeof(int) && keytype != typeof(uint) && keytype != typeof(long) && keytype != typeof(ulong) && keytype != typeof(short) && keytype != typeof(ushort) && keytype != typeof(Guid) && keytype != typeof(string))
            {
                throw new Exception("Invalid return type");
            }
            var name = GetTableName(entityToInsert);
            var sb = new StringBuilder();
            sb.AppendFormat(" insert into {0} ", name);
            sb.Append(" (");
            BuildInsertParameters(sb, type);
            sb.Append(") ");
            sb.Append("values");
            sb.Append(" (");
            BuildInsertValues(sb, type);
            sb.Append(")");
            return sb.ToString();
        }
        #endregion

        #region [3、获取表名称]
        /// <summary>
        /// 表转化器
        /// </summary>
        public interface ITableNameResolver
        {
            /// <summary>
            /// 表名称转化器
            /// </summary>
            /// <param name="type"></param>
            /// <returns></returns>
            string ResolveTableName(Type type);
        }

        /// <summary>
        /// 表名称转化器
        /// </summary>
        public class TableNameResolver : ITableNameResolver
        {
            /// <summary>
            /// 表名称转化器
            /// </summary>
            /// <param name="type"></param>
            /// <returns></returns>
            public virtual string ResolveTableName(Type type)
            {
                var tableName = Encapsulate(type.Name);

                var tableattr = type.GetCustomAttributes(true).SingleOrDefault(attr => attr.GetType().Name == typeof(TableAttribute).Name) as dynamic;
                if (tableattr != null)
                {
                    tableName = Encapsulate(tableattr.Name);
                    try
                    {
                        if (!string.IsNullOrEmpty(tableattr.Schema))
                        {
                            string schemaName = Encapsulate(tableattr.Schema);
                            tableName = $"{schemaName}.{tableName}";
                        }
                    }
                    catch (RuntimeBinderException)
                    {
                        //Schema doesn't exist on this attribute.
                    }
                }

                return tableName;
            }
        }

        /// <summary>
        /// 获取表名称
        /// </summary>
        /// <param name="databaseword"></param>
        /// <returns></returns>
        private static string Encapsulate(string databaseword)
        {
            return string.Format(_encapsulation, databaseword);
        }

        /// <summary>
        /// 获取表名称，默认是类名称，或者有表的特性（Persistent）；
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static string GetTableName(object entity)
        {
            var type = entity.GetType();
            return GetTableName(type);
        }

        /// <summary>
        /// 获取表名称，默认是类名称，或者有表的特性（Persistent）
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string GetTableName(Type type)
        {
            Tuple<string, string> tempTableName;
            if (TableNames.TryGetValue(type, out tempTableName))
            {
                if (tempTableName.Item2 == _dialect) return tempTableName.Item1;
            }
            var tableName = SelftableNameResolver.ResolveTableName(type);
            TableNames.TryAdd(type, new Tuple<string, string>(tableName, _dialect));

            return tableName;
        }
        #endregion

        #region [4、获取列名称]
        /// <summary>
        /// Interface Column name resolver
        /// </summary>
        public interface IColumnNameResolver
        {
            /// <summary>
            /// resolve column name
            /// </summary>
            /// <param name="propertyInfo"></param>
            /// <returns></returns>
            string ResolveColumnName(PropertyInfo propertyInfo);
        }

        /// <summary>
        /// 列名解决方案
        /// </summary>
        public class ColumnNameResolver : IColumnNameResolver
        {
            /// <summary>
            /// resolve column name
            /// </summary>
            /// <param name="propertyInfo"></param>
            /// <returns></returns>
            public virtual string ResolveColumnName(PropertyInfo propertyInfo)
            {
                var columnName = Encapsulate(propertyInfo.Name);

                var columnattr = propertyInfo.GetCustomAttributes(true).SingleOrDefault(attr => attr.GetType().Name == typeof(ColumnAttribute).Name) as dynamic;
                if (columnattr != null)
                {
                    columnName = Encapsulate(columnattr.Name);
                }
                return columnName;
            }
        }
        #endregion

        #region [5、获取主键]
        /// <summary>
        /// 获取属性为Id或者标识了主键特性（Key）
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static IEnumerable<PropertyInfo> GetIdProperties(object entity)
        {
            var type = entity.GetType();
            return GetIdProperties(type);
        }

        /// <summary>
        /// 获取主键
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static IEnumerable<PropertyInfo> GetIdProperties(Type type)
        {
            var tp = type.GetProperties().Where(p => p.GetCustomAttributes(true).Any(attr => attr.GetType().Name == typeof(KeyAttribute).Name)).ToList();
            return tp.Any() ? tp : type.GetProperties().Where(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
        }
        #endregion

        #region [6、获取插入参数集合]

        /// <summary>
        /// 获取插入的参数集合
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="type"></param>
        private static void BuildInsertParameters(StringBuilder sb, Type type)
        {
            var props = GetScaffoldableProperties(type);
            for (int i = 0; i < props.Count; i++)
            {
                var property = props.ElementAt(i);
                var customAttributes = property.GetCustomAttributes(true);
                if (property.PropertyType != typeof(Guid)
                    && customAttributes.Any(f => f.GetType().Name == typeof(KeyAttribute).Name)
                    && customAttributes.All(r => r.GetType().Name != typeof(IdentitySeedAttribute).Name)) continue;
                if (customAttributes.Any(attr => attr.GetType().Name == typeof(NonPersistentAttribute).Name)) continue;
                if (!property.CanWrite) continue;
                sb.Append(GetColumnName(property));
                if (i < props.Count - 1) sb.Append(", ");
            }
            if (sb.ToString().EndsWith(", ")) sb.Remove(sb.Length - 2, 2);
        }

        /// <summary>
        /// 获取插入的值
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="type"></param>
        private static void BuildInsertValues(StringBuilder sb, Type type)
        {
            var props = GetScaffoldableProperties(type);
            for (int i = 0; i < props.Count; i++)
            {
                var property = props.ElementAt(i);
                var customAttributes = property.GetCustomAttributes(true);
                if (property.PropertyType != typeof(Guid)
                    && customAttributes.Any(f => f.GetType().Name == typeof(KeyAttribute).Name)
                    && customAttributes.All(r => r.GetType().Name != typeof(IdentitySeedAttribute).Name)) continue;
                if (customAttributes.Any(attr => attr.GetType().Name == typeof(NonPersistentAttribute).Name)) continue;
                if (!property.CanWrite) continue;
                sb.AppendFormat("@{0}", property.Name);
                if (i < props.Count - 1) sb.Append(", ");
            }
            if (sb.ToString().EndsWith(", ")) sb.Remove(sb.Length - 2, 2);
        }

        /// <summary>  
        /// 根据sql语句和实体对象自动生成参数化查询SqlParameter列表  
        /// </summary>  
        /// <typeparam name="T">实体对象类型</typeparam>
        /// <param name="obj">实体对象</param>  
        /// <returns>SqlParameter列表</returns>  
        private static List<SqlParameter> ObjectToParameters<T>(T obj)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            var props = GetScaffoldableProperties(obj.GetType());
            for (var i = 0; i < props.Count; i++)
            {
                var property = props.ElementAt(i);
                var customAttributes = property.GetCustomAttributes(true);
                if (property.PropertyType != typeof(Guid)
                    && customAttributes.Any(f => f.GetType().Name == typeof(KeyAttribute).Name)
                    && customAttributes.All(r => r.GetType().Name != typeof(IdentitySeedAttribute).Name)) continue;
                if (customAttributes.Any(attr => attr.GetType().Name == typeof(NonPersistentAttribute).Name)) continue;
                if (!property.CanWrite) continue;
                object paramsValue = null;
                if (property.PropertyType.Name == "String" || property.PropertyType.IsValueType)
                {
                    //傻逼的时间类型特殊处理
                    if (property.PropertyType == typeof(DateTime))
                    {
                        if (((DateTime)props[i].GetValue(obj, null)) < new DateTime(1900, 1, 1))
                        {
                            paramsValue = DBNull.Value;
                        }
                        else
                        {
                            paramsValue = props[i].GetValue(obj, null) ?? DBNull.Value;
                        }
                    }
                    else
                    {
                        paramsValue = props[i].GetValue(obj, null) ?? DBNull.Value;
                    }
                }
                else
                {
                    var entityValue = props[i].GetValue(obj, null);
                    if (entityValue != null)
                    {
                        IEnumerable<PropertyInfo> referenceProps = property.PropertyType.GetProperties();
                        var referenceKey =
                            referenceProps.FirstOrDefault(
                                f => f.GetCustomAttributes(typeof(KeyAttribute), true).Length > 0);
                        if (referenceKey != null)
                        {
                            var tempParamsValue = referenceKey.GetValue(entityValue, null);
                            if (referenceKey.PropertyType.Name == typeof(string).Name ||
                                referenceKey.PropertyType.Name == typeof(Guid).Name)
                            {
                                paramsValue = tempParamsValue ?? DBNull.Value;
                            }
                            else
                            {
                                paramsValue = Convert.ToInt64(tempParamsValue) < 1 ? DBNull.Value : tempParamsValue;
                            }
                        }
                    }
                    else
                    {
                        paramsValue = DBNull.Value;
                    }
                }
                parameters.Add(new SqlParameter { ParameterName = "@" + props[i].Name, Value = paramsValue });
            }

            return parameters;
        }
        #endregion

        #region [7、获取可存储参数]

        /// <summary>
        /// 获取可存储的简单类型
        /// </summary>
        /// <returns></returns>
        public static List<PropertyInfo> GetScaffoldableProperties(Type type)
        {
            string key = $"{type.FullName}";
            List<PropertyInfo> typeProperCaches;
            if (TypePropertyCaches.TryGetValue(key, out typeProperCaches))
            {
                return typeProperCaches;
            }
            var props = type.GetProperties();
            typeProperCaches =
                props.Where(
                    e =>
                        e.GetCustomAttributes(typeof(NonPersistentAttribute), true).Length < 1
                        && (e.PropertyType.IsSimpleType() ||
                            e.GetCustomAttributes(typeof(SaveEntityAttribute), true).Length > 0)
                        && !ExcludeFieldList.Contains(e.Name)).ToList();
            TypePropertyCaches.TryAdd(key, typeProperCaches);
            return typeProperCaches;
        }

        /// <summary>
        /// 获取列名称
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        private static string GetColumnName(PropertyInfo propertyInfo)
        {
            string key = $"{propertyInfo.DeclaringType}.{propertyInfo.Name}";
            Tuple<string, string> tempColumn;
            if (ColumnNames.TryGetValue(key, out tempColumn))
            {
                if (tempColumn.Item2 == _dialect) return tempColumn.Item1;
            }
            var columnName = SelfcolumnNameResolver.ResolveColumnName(propertyInfo);
            ColumnNames.TryAdd(key, new Tuple<string, string>(columnName, _dialect));

            return columnName;
        }

        /// <summary>
        ///根据当前时间生成GUID
        /// <para> http://stackoverflow.com/questions/1752004/sequential-guid-generator-c-sharp </para>
        /// </summary>
        /// <returns></returns>
        private static Guid SequentialGuid()
        {
            var tempGuid = Guid.NewGuid();
            var bytes = tempGuid.ToByteArray();
            var time = DateTime.Now;
            bytes[3] = (byte)time.Year;
            bytes[2] = (byte)time.Month;
            bytes[1] = (byte)time.Day;
            bytes[0] = (byte)time.Hour;
            bytes[5] = (byte)time.Minute;
            bytes[4] = (byte)time.Second;
            return new Guid(bytes);
        }
        #endregion

        #region [8、创建更新SQL]

        /// <summary>
        /// 更新实体信息
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entityToUpdate"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Update<TEntity>(TEntity entityToUpdate, bool transaction = false, int? commandTimeout = null)
        {
            var idProps = GetIdProperties(entityToUpdate).ToList();
            if (!idProps.Any())
                throw new ArgumentException("Entity must have at least one [Key] or Id property");
            var name = GetTableName(entityToUpdate);
            var sb = new StringBuilder();
            sb.AppendFormat("update {0}", name);

            sb.AppendFormat(" set ");
            BuildUpdateSet(entityToUpdate, sb);
            sb.Append(" where ");
            BuildWhere(sb, idProps, entityToUpdate);
            var paramters = ObjectToParameters(entityToUpdate);
            return ExecuteCommand(sb.ToString(), paramters, transaction, commandTimeout);
        }
        #endregion

        #region [9、获取更新列表]

        /// <summary>
        /// 获取更新的set语句块
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityToUpdate"></param>
        /// <param name="sb"></param>
        private static void BuildUpdateSet<T>(T entityToUpdate, StringBuilder sb)
        {
            var nonIdProps = GetUpdateableProperties(entityToUpdate).ToArray();

            for (var i = 0; i < nonIdProps.Length; i++)
            {
                var property = nonIdProps[i];

                sb.AppendFormat("{0} = @{1}", GetColumnName(property), property.Name);
                if (i < nonIdProps.Length - 1)
                    sb.AppendFormat(", ");
            }
        }
        #endregion

        #region [10、获取更新字段列表]

        /// <summary>
        /// 获取更新的字段列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static IEnumerable<PropertyInfo> GetUpdateableProperties<T>(T entity)
        {
            var type = entity.GetType();
            var entityBaseProperty = GetScaffoldableProperties(type);
            //remove ones with key attribute
            var updateableProperties =
                entityBaseProperty.Where(
                    p =>
                        p.GetCustomAttributes(true).Any(attr => attr.GetType().Name == typeof(KeyAttribute).Name) ==
                        false && p.CanWrite);

            return updateableProperties;
        }
        #endregion

        #region [11、构建更新条件]

        /// <summary>
        /// 构建where条件
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="sb"></param>
        /// <param name="idProps"></param>
        /// <param name="sourceEntity"></param>
        /// <param name="whereConditions"></param>
        private static void BuildWhere<TEntity>(StringBuilder sb, IEnumerable<PropertyInfo> idProps, TEntity sourceEntity, object whereConditions = null)
        {
            var propertyInfos = idProps.ToArray();
            var type = sourceEntity.GetType();
            var sourceProperties = GetScaffoldableProperties(type).ToArray();
            for (var i = 0; i < propertyInfos.Count(); i++)
            {
                var useIsNull = false;

                var propertyToUse = propertyInfos.ElementAt(i);
                for (var x = 0; x < sourceProperties.Count(); x++)
                {
                    if (sourceProperties.ElementAt(x).Name == propertyInfos.ElementAt(i).Name)
                    {
                        propertyToUse = sourceProperties.ElementAt(x);

                        if (whereConditions != null
                            && propertyInfos.ElementAt(i).CanRead
                            && (propertyInfos.ElementAt(i).GetValue(whereConditions, null) == null
                                || propertyInfos.ElementAt(i).GetValue(whereConditions, null) == DBNull.Value))
                        {
                            useIsNull = true;
                        }
                        break;
                    }
                }
                sb.AppendFormat(
                    useIsNull ? "{0} is null" : "{0} = @{1}",
                    GetColumnName(propertyToUse),
                    propertyInfos.ElementAt(i).Name);

                if (i < propertyInfos.Count() - 1)
                    sb.AppendFormat(" and ");
            }
        }
        #endregion

        #region [12、获取禁用外键约束]

        /// <summary>
        /// 获取禁用外键约束sql
        /// </summary>
        /// <returns></returns>
        public static string GetDisabledForeignKeySql()
        {
            var disableSql = @"DECLARE
        @nocheckSql NVARCHAR (MAX)
    SET @nocheckSql = (
        SELECT
            'alter table dbo.[' + b.name + '] nocheck constraint [' + a.name + '];'
        FROM
            sysobjects a,
            sysobjects b
        WHERE
            a.xtype = 'f'
        AND a.parent_obj = b.id
        AND b.xtype = 'u' FOR xml PATH('')
	) select @nocheckSql";
            var result = GetScalar(disableSql);
            return result.ToString();
        }
        #endregion

        #region [13、获取启用外键约束]

        /// <summary>
        /// 获取启用外键约束sql
        /// </summary>
        /// <returns></returns>
        public static string GetEnabledForeignKeySql()
        {
            var disableSql = @"DECLARE
		@checkSql NVARCHAR (MAX)
	SET @checkSql = (
		SELECT
			'alter table dbo.[' + b.name + '] check constraint [' + a.name + '];'
		FROM
			sysobjects a,
			sysobjects b
		WHERE
			a.xtype = 'f'
		AND a.parent_obj = b.id
		AND b.xtype = 'u' FOR xml PATH ('')
	) select @checkSql";
            var result = GetScalar(disableSql);
            return result.ToString();
        }

        #endregion

        #region [14、获取删除外键约束]

        /// <summary>
        /// 获取删除外键约束的SQL
        /// </summary>
        /// <returns></returns>
        public static string GetDeleteForeignKeySql()
        {
            var disableSql = @"DECLARE 
		@delSql nvarchar (MAX)
        SET @delSql = (
		SELECT
			'alter table [' + O.name + '] drop constraint [' + F.name + '];'
		FROM
			sysobjects O,
			sys.foreign_keys F
		WHERE
			F.parent_object_id = O.id FOR xml path ('')
	) select @delSql ";
            var result = GetScalar(disableSql);
            return result.ToString();
        }
        #endregion

        #region [15、获取重建外键约束]

        /// <summary>
        /// 获取重建外键约束SQL
        /// </summary>
        /// <returns></returns>
        public static string GetReCreatForeignKeySql()
        {
            var disableSql = @"DECLARE 
		@createSql nvarchar (MAX)
        SET @createSql = (
		SELECT
			'ALTER TABLE [' + OBJECT_NAME(k.parent_object_id) + '] ADD CONSTRAINT [' + k.name + '] FOREIGN KEY ([' + COL_NAME(
				k.parent_object_id,
				c.parent_column_id
			) + ']) REFERENCES [' + OBJECT_NAME(k.referenced_object_id) + ']([' + COL_NAME(
				k.referenced_object_id,
				key_index_id
			) + '])' + CASE k.delete_referential_action
		WHEN 0 THEN
			''
		WHEN 1 THEN
			' ON DELETE CASCADE '
		WHEN 2 THEN
			' ON DELETE SET NULL '
		WHEN 3 THEN
			' ON DELETE SET DEFAULT '
		END + CASE k.update_referential_action
		WHEN 0 THEN
			''
		WHEN 1 THEN
			' ON UPDATE CASCADE '
		WHEN 2 THEN
			' ON UPDATE SET NULL '
		WHEN 3 THEN
			' ON UPDATE SET DEFAULT'
		END + ';'
		FROM
			sys.foreign_keys k,
			sys.foreign_key_columns c
		WHERE
			c.constraint_object_id = k.object_id FOR xml path ('')
	) select @createSql ";
            var result = GetScalar(disableSql);
            return result.ToString();
        }
        #endregion

        #region [16、获得数据]

        /// <summary>
        /// 获取单条记录
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="searchModel">主键Id</param>
        /// <returns></returns>
        public static T Get<T>(object searchModel) where T : class, new()
        {
            var currenttype = typeof (T);
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
                if(!propertyInfos.ElementAt(i).CanWrite) continue;
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

    #region [1、使用自定义的表格标识]
    /// <summary>
    /// Optional Table attribute.
    /// You can use the System.ComponentModel.DataAnnotations version in its place to specify the table name of a poco
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        /// <summary>
        /// Optional Table attribute.
        /// </summary>
        /// <param name="tableName"></param>
        public TableAttribute(string tableName)
        {
            Name = tableName;
        }
        /// <summary>
        /// Name of the table
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Name of the schema
        /// </summary>
        public string Schema { get; set; }
    }

    /// <summary>
    /// Optional Column attribute.
    /// You can use the System.ComponentModel.DataAnnotations version in its place to specify the table name of a poco
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        /// <summary>
        /// Optional Column attribute.
        /// </summary>
        /// <param name="columnName"></param>
        public ColumnAttribute(string columnName)
        {
            Name = columnName;
        }
        /// <summary>
        /// Name of the column
        /// </summary>
        public string Name { get; private set; }
    }
    #endregion

    #region [2、Type扩展方法]
    /// <summary>
    /// Type extension
    /// </summary>
    internal static class TypeExtension
    {
        /// <summary>
        /// You can't insert or update complex types. Lets filter them out.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsSimpleType(this Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            type = underlyingType ?? type;
            var simpleTypes = new List<Type>
                               {
                                   typeof(byte),
                                   typeof(sbyte),
                                   typeof(short),
                                   typeof(ushort),
                                   typeof(int),
                                   typeof(uint),
                                   typeof(long),
                                   typeof(ulong),
                                   typeof(float),
                                   typeof(double),
                                   typeof(decimal),
                                   typeof(bool),
                                   typeof(string),
                                   typeof(char),
                                   typeof(Guid),
                                   typeof(DateTime),
                                   typeof(DateTimeOffset),
                                   typeof(byte[]),
                                   typeof(TimeSpan)
                               };
            return simpleTypes.Contains(type) || type.IsEnum;
        }
    }
    #endregion

    #region [3、设置是否是父类的标识种子]

    /// <summary>
    /// 标识是否父类的标识种子，也就是说作为子类的外键
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IdentitySeedAttribute : Attribute
    {
    }
    #endregion

    #region [4、是否保存关联中的实体的主键（单一主键）]

    /// <summary>
    /// 是否保存关联中的实体的主键（单一主键）
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SaveEntityAttribute : Attribute
    {
    }
    #endregion
}
