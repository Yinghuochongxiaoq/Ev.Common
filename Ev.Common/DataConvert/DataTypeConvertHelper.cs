/**============================================================
* 命名空间: Ev.Common.DataConvert
*
* 功 能： N/A
* 类 名： DataTypeConvertHelper
*
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
* V0.01 2017/7/7 16:08:23 FreshMan 初版
*
* Copyright (c) 2017 Lir Corporation. All rights reserved.
*==============================================================
*==此技术信息为本公司机密信息,未经本公司书面同意禁止向第三方披露==
*==版权所有：重庆慧都科技有限公司                             ==
*==============================================================
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Ev.Common.DataConvert
{
    /// <summary>
    /// DataType convert helper
    /// </summary>
    public static class DataTypeConvertHelper
    {
        #region [0、Check Entity have reflect]
        /// <summary>
        /// 检测实体中是否存在引用，存在返回false，不存在，返回true
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="errorMessage"></param>
        /// <author>FreshMan</author>
        /// <creattime>2017-06-21</creattime>
        /// <returns></returns>
        public static bool CheckEntityRelevanceIsEmpty<T>(T entity, out string errorMessage)
        {
            errorMessage = string.Empty;
            var properties = typeof(T).GetProperties();
            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.PropertyType.IsGenericType)
                {
                    var property = typeof(T).GetProperty(propertyInfo.Name);
                    if (property != null)
                    {
                        var t = property.GetValue(entity, null);
                        try
                        {
                            ICollection ilist = t as ICollection;
                            if (ilist != null && ilist.Count > 0)
                            {
                                foreach (object obj in ilist)
                                {
                                    errorMessage = typeof(T).Name + " data template has been used by type:" + obj.GetType().FullName + " you can't delete it.";
                                    break;
                                }
                            }

                        }
                        catch (Exception exception)
                        {
                            errorMessage = exception.Message + exception.InnerException?.Message;
                        }
                    }
                }
            }
            return string.IsNullOrEmpty(errorMessage);
        }
        #endregion

        #region [1、IEnumerable to dataTable]

        #region [1.1 比较慢的方法]
        /// <summary>
        /// <para>集合转化为表格</para>
        /// <para>T中应该只包含值类型，对应的DataTable自动匹配列名相同的属性</para>
        /// <para>当数据量大于100时，请用<see cref="ToDataTable{TSource}"/></para>
        /// </summary>
        /// <typeparam name="T">类型中不应该包含有引用类型</typeparam>
        /// <param name="entityList">转换的集合</param> 
        /// <author>FreshMan</author>
        /// <creattime>2017-06-26</creattime>
        /// <returns></returns>
        public static DataTable ToDataTableSlowly<T>(IList<T> entityList)
        {
            if (entityList == null) return null;
            var dt = CreateTable<T>();
            Type entityType = typeof(T);
            var properties = entityType.GetProperties();
            foreach (T item in entityList)
            {
                DataRow row = dt.NewRow();
                foreach (var property in properties)
                {
                    if (!property.CanRead || (!property.PropertyType.IsValueType && property.PropertyType != typeof(string))) continue;
                    row[property.Name] = property.GetValue(item, null);
                }
                dt.Rows.Add(row);
            }
            return dt;
        }

        /// <summary>
        /// <para>创建表格</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        /// <author>FreshMan</author>
        /// <creattime>2017-06-26</creattime>
        /// <returns></returns>
        private static DataTable CreateTable<T>(string tableName = null)
        {
            Type entityType = typeof(T);
            PropertyDescriptorCollection propertyies = TypeDescriptor.GetProperties(entityType);
            DataTable dt = new DataTable(tableName);
            foreach (PropertyDescriptor prop in propertyies)
            {
                dt.Columns.Add(prop.Name);
            }
            return dt;
        }
        #endregion

        #region [1.2 比较快的方法]
        /// <summary>
        /// <para>Creates a DataTable from an IEnumerable</para>
        /// <para>当数据量小于100时，请用<see cref="ToDataTableSlowly{TSource}"/></para>
        /// </summary>
        /// <typeparam name="TSource">The Generic type of the Collection</typeparam>
        /// <param name="collection"></param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable<TSource>(IEnumerable<TSource> collection)
        {
            DataTable dt = DataTableCreator<TSource>.GetDataTable();
            Func<TSource, object[]> map = DataRowMapperCache<TSource>.GetDataRowMapper(dt);

            foreach (TSource item in collection)
            {
                dt.Rows.Add(map(item));
            }
            return dt;
        }

        /// <summary>
        /// 使用泛型类型创建一个同样字段名的DataTable结构
        /// </summary>
        /// <typeparam name="TSource">泛型类型</typeparam>
        /// <returns>DataTable</returns>
        static internal DataTable CreateDataTable<TSource>()
        {
            DataTable dt = new DataTable();
            foreach (FieldInfo sourceMember in typeof(TSource).GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                dt.AddTableColumn(sourceMember, sourceMember.FieldType);
            }

            foreach (PropertyInfo sourceMember in typeof(TSource).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (sourceMember.CanRead)
                {
                    dt.AddTableColumn(sourceMember, sourceMember.PropertyType);
                }
            }
            return dt;
        }

        /// <summary>
        /// 只将值类型和string类型添加一列到DataTable中
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="sourceMember">列对象</param>
        /// <param name="memberType">列类型</param>
        private static void AddTableColumn(this DataTable dt, MemberInfo sourceMember, Type memberType)
        {
            if ((memberType.IsValueType || memberType == typeof(string)))
            {
                DataColumn dc;
                string fieldName = GetFieldNameAttribute(sourceMember);
                if (string.IsNullOrWhiteSpace(fieldName))
                {
                    fieldName = sourceMember.Name;
                }
                if (Nullable.GetUnderlyingType(memberType) == null)
                {
                    dc = new DataColumn(fieldName, memberType) { AllowDBNull = !memberType.IsValueType };
                }
                else
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    dc = new DataColumn(fieldName, Nullable.GetUnderlyingType(memberType)) { AllowDBNull = true };
                }
                dt.Columns.Add(dc);
            }
        }

        /// <summary>
        /// 获取Field特性，如果存在
        /// </summary>
        /// <param name="member">MemberInfo</param>
        /// <returns>String</returns>
        private static string GetFieldNameAttribute(MemberInfo member)
        {
            if (member.GetCustomAttributes(typeof(FieldNameAttribute), true).Any())
            {
                return ((FieldNameAttribute)member.GetCustomAttributes(typeof(FieldNameAttribute), true)[0]).FieldName;
            }
            return string.Empty;
        }

        /// <summary>
        /// 检测字段名称或者设置的field特性名称是否匹配
        /// </summary>
        /// <param name="member">The Member of the Instance to check</param>
        /// <param name="name">The Name to compare with</param>
        /// <returns>True if Fields match</returns>
        /// <remarks>FieldNameAttribute takes precedence over TargetMembers name.</remarks>
        private static bool MemberMatchesName(MemberInfo member, string name)
        {
            string fieldnameAttribute = GetFieldNameAttribute(member);
            return fieldnameAttribute.ToLower() == name.ToLower() || member.Name.ToLower() == name.ToLower();
        }

        /// <summary>
        /// 创建表达式
        /// </summary>
        /// <param name="sourceInstanceExpression"></param>
        /// <param name="sourceMember"></param>
        /// <returns></returns>
        private static Expression GetSourceValueExpression(ParameterExpression sourceInstanceExpression, MemberInfo sourceMember)
        {
            MemberExpression memberExpression = Expression.PropertyOrField(sourceInstanceExpression, sourceMember.Name);
            Expression sourceValueExpression;

            // ReSharper disable once AssignNullToNotNullAttribute
            if (Nullable.GetUnderlyingType(sourceMember.ReflectedType) == null)
            {
                sourceValueExpression = Expression.Convert(memberExpression, typeof(object));
            }
            else
            {
                sourceValueExpression = Expression.Condition(
                    Expression.Property(Expression.Constant(sourceInstanceExpression), "HasValue"),
                    memberExpression,
                    Expression.Constant(DBNull.Value),
                    typeof(object));
            }
            return sourceValueExpression;
        }

        /// <summary>
        /// 创建一个委托，该TSource的实例映射到一个提供数据表的ItemArray
        /// </summary>
        /// <typeparam name="TSource">The Generic Type to map from</typeparam>
        /// <param name="dt">The DataTable to map to</param>
        /// <returns>Func(Of TSource, Object())</returns>
        static internal Func<TSource, object[]> CreateDataRowMapper<TSource>(DataTable dt)
        {
            Type sourceType = typeof(TSource);
            ParameterExpression sourceInstanceExpression = Expression.Parameter(sourceType, "SourceInstance");
            List<Expression> values = new List<Expression>();

            foreach (DataColumn col in dt.Columns)
            {
                foreach (FieldInfo sourceMember in sourceType.GetFields(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (MemberMatchesName(sourceMember, col.ColumnName))
                    {
                        values.Add(GetSourceValueExpression(sourceInstanceExpression, sourceMember));
                        break;
                    }
                }
                foreach (PropertyInfo sourceMember in sourceType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (sourceMember.CanRead && MemberMatchesName(sourceMember, col.ColumnName))
                    {
                        values.Add(GetSourceValueExpression(sourceInstanceExpression, sourceMember));
                        break;
                    }
                }
            }
            // ReSharper disable once AssignNullToNotNullAttribute
            NewArrayExpression body = Expression.NewArrayInit(Type.GetType("System.Object"), values);
            return Expression.Lambda<Func<TSource, object[]>>(body, sourceInstanceExpression).Compile();
        }

        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        private sealed class DataRowMapperCache<TSource>
        {
            private DataRowMapperCache() { }

            // ReSharper disable once StaticMemberInGenericType
            private static readonly object LockObject = new object();
            private static Func<TSource, object[]> _mapper;

            static internal Func<TSource, object[]> GetDataRowMapper(DataTable dt)
            {
                if (_mapper == null)
                {
                    lock (LockObject)
                    {
                        if (_mapper == null)
                        {
                            _mapper = CreateDataRowMapper<TSource>(dt);
                        }
                    }
                }
                return _mapper;
            }
        }

        /// <summary>
        /// 创建实体对应的DataTable结构
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        private sealed class DataTableCreator<TSource>
        {
            private DataTableCreator() { }

            // ReSharper disable once StaticMemberInGenericType
            private static readonly object LockObject = new object();
            // ReSharper disable once StaticMemberInGenericType
            private static DataTable _emptyDataTable;
            static internal DataTable GetDataTable()
            {
                if (_emptyDataTable == null)
                {
                    lock (LockObject)
                    {
                        if (_emptyDataTable == null)
                        {
                            _emptyDataTable = CreateDataTable<TSource>();
                        }
                    }
                }
                return _emptyDataTable.Clone();
            }
        }
        #endregion
        #endregion

        #region [2、DataTable to generics]

        #region [2.1 比较慢的方法]
        /// <summary>
        /// <para>表格转集合</para>
        /// <para>DataTable中的列名称自动匹配<see cref="TResult"/>中的属性</para>
        /// <para>当数据量大于100时，请用<see cref="ToListFast{TResult}"/></para>
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="dt"></param>
        /// <author>FreshMan</author>
        /// <creattime>2017-06-26</creattime>
        /// <returns></returns>
        private static List<TResult> ToListSlowly<TResult>(DataTable dt) where TResult : class, new()
        {
            //初始化转换对象
            List<TResult> resulteList = new List<TResult>();
            if (dt == null) return default(List<TResult>);
            //获取此模型的公共属性
            PropertyInfo[] propertys = typeof(TResult).GetProperties();
            DataColumnCollection columns = dt.Columns;
            int m = 0;
            foreach (DataRow dataRow in dt.Rows)
            {
                m++;
                TResult t = new TResult();
                foreach (PropertyInfo p in propertys)
                {
                    string columnName = p.Name;
                    if (!columns.Contains(columnName)) continue;
                    //判断此属性是否有Setting或columnName值是否为空
                    object value = dataRow[columnName];
                    if (!p.CanWrite || value is DBNull || value == DBNull.Value || (!p.PropertyType.IsValueType && p.PropertyType != typeof(string))) continue;
                    try
                    {
                        switch (p.PropertyType.ToString())
                        {
                            case "System.String":
                                p.SetValue(t, Convert.ToString(value), null);
                                break;
                            case "System.Int32":
                                p.SetValue(t, Convert.ToInt32(value), null);
                                break;
                            case "System.Int64":
                                p.SetValue(t, Convert.ToInt64(value), null);
                                break;
                            case "System.DateTime":
                            case "System.Nullable`1[System.DateTime]":
                                p.SetValue(t, Convert.ToDateTime(value), null);
                                break;
                            case "System.Boolean":
                                p.SetValue(t, Convert.ToBoolean(value), null);
                                break;
                            case "System.Double":
                                p.SetValue(t, Convert.ToDouble(value), null);
                                break;
                            case "System.Decimal":
                                p.SetValue(t, Convert.ToDecimal(value), null);
                                break;
                            case "System.TimeSpan":
                                p.SetValue(t, TimeSpan.Parse(value.ToString()), null);
                                break;
                            default:
                                if (typeof(Enum).IsAssignableFrom(p.PropertyType))
                                {
                                    p.SetValue(t, Convert.ToInt32(value), null);
                                }
                                else
                                    p.SetValue(t, value, null);
                                break;

                        }
                    }
                    catch (Exception e)
                    {
                        string msg = "第" + m + "行-" + p.PropertyType.Name + "列" + "数据有问题，请重新设置！\n" + e.Message;
                        throw new Exception(msg);

                    }
                }
                resulteList.Add(t);
            }
            return resulteList;
        }
        #endregion

        #region [2.2 比较快的方法]
        /// <summary>
        /// <para>表格转集合</para>
        /// <para>DataTable中的列名称自动匹配<see cref="TResult"/>中的属性</para>
        /// <para>当数据量小于100是，请用<see cref="ToListSlowly{TResult}"/></para>
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="table"></param>
        /// <author>FreshMan</author>
        /// <creattime>2017-06-26</creattime>
        /// <returns></returns>
        private static List<TResult> ToListFast<TResult>(DataTable table) where TResult : class, new()
        {
            List<TResult> list = new List<TResult>();
            if (table == null || table.Rows.Count < 1) return list;
            DataTable dt = DataTableCreator<TResult>.GetDataTable();
            var oldColums = table.Columns;
            var newColums = dt.Columns;
            DataTableEntityBuilder<TResult> eblist = null;
            //行计数器
            long rowNum = 0;
            foreach (DataRow dataRow in table.Rows)
            {
                var dtRow = dt.NewRow();
                var flag = false;
                foreach (DataColumn dataColumn in oldColums.Cast<DataColumn>().Where(dataColumn => newColums.Contains(dataColumn.ColumnName)))
                {
                    flag = true;
                    try
                    {
                        dtRow[dataColumn.ColumnName] = dataRow[dataColumn.ColumnName];
                    }
                    catch (ArgumentException argumentException)
                    {
                        // ReSharper disable once PossibleIntendedRethrow
                        var exception =
                            new ArgumentException(
                                argumentException.Message + "context data is " +
                                dataRow.ItemArray.Aggregate(
                                    (current, temp) => current.ToString() + "," + temp.ToString()), argumentException);
                        throw exception;
                    }
                    catch (Exception exception)
                    {
                        // ReSharper disable once PossibleIntendedRethrow
                        throw exception;
                    }
                }
                if (flag && rowNum == 0)
                {
                    eblist = DataTableEntityBuilder<TResult>.CreateBuilder(dtRow);
                }
                if (!flag) continue;
                rowNum++;
                // ReSharper disable once PossibleNullReferenceException
                TResult tempInfo = eblist.Build(dtRow);
                list.Add(tempInfo);
            }
            dt.Dispose();
            return list;
        }

        /// <summary>
        /// <para>表格转集合</para>
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="table"></param>
        /// <author>FreshMan</author>
        /// <creattime>2017-06-29</creattime>
        /// <returns></returns>
        public static List<TResult> ToList<TResult>(DataTable table) where TResult : class, new()
        {
            //初始化转换对象
            List<TResult> list = new List<TResult>();
            if (table == null || table.Rows.Count < 1) return list;
            return table.Rows.Count > 100 ? ToListFast<TResult>(table) : ToListSlowly<TResult>(table);
        }
        #endregion
        #endregion

        #region [3、To int value]
        /// <summary>
        /// 转换成int
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static int ToInt(object val)
        {
            return ToInt(val, 0);
        }

        /// <summary>
        /// 转换成int
        /// </summary>
        /// <param name="val"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int ToInt(object val, int defaultValue)
        {
            int num;
            if ((val == null) || (val == DBNull.Value))
            {
                return defaultValue;
            }
            if (val is int)
            {
                return (int)val;
            }
            if (!int.TryParse(val.ToString().Trim(), NumberStyles.Number, null, out num))
            {
                return defaultValue;
            }
            return num;
        }
        #endregion

        #region [4、To long value]
        /// <summary>
        /// 转换成long
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static long ToLong(object val)
        {
            return ToLong(val, 0L);
        }

        /// <summary>
        /// 转换成long
        /// </summary>
        /// <param name="val"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static long ToLong(object val, long defaultValue)
        {
            long num;
            if ((val == null) || (val == DBNull.Value))
            {
                return defaultValue;
            }
            if (val is long)
            {
                return (long)val;
            }
            if (!long.TryParse(val.ToString(), out num))
            {
                return defaultValue;
            }
            return num;
        }

        /// <summary>
        /// 转换成long?
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static long? ToLongNullable(object val)
        {
            long num = ToLong(val);
            if (num.Equals(0L))
            {
                return null;
            }
            return num;
        }
        #endregion
    }

    /// <summary>
    /// 字段别名特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class FieldNameAttribute : Attribute
    {
        /// <summary>
        /// 别名
        /// </summary>
        public string FieldName { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fieldName"></param>
        public FieldNameAttribute(string fieldName)
        {
            FieldName = fieldName;
        }
    }

    /// <summary>
    /// 创建转换的委托
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class DataTableEntityBuilder<TEntity>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly MethodInfo GetValueMethod = typeof(DataRow).GetMethod("get_Item", new[] { typeof(int) });
        // ReSharper disable once StaticMemberInGenericType
        private static readonly MethodInfo IsDbNullMethod = typeof(DataRow).GetMethod("IsNull", new[] { typeof(int) });
        private delegate TEntity Load(DataRow dataRecord);

        private Load _handler;
        private DataTableEntityBuilder() { }

        /// <summary>
        /// Creat build new method and cache.
        /// </summary>
        /// <param name="dataRecord"></param>
        /// <returns></returns>
        public TEntity Build(DataRow dataRecord)
        {
            return _handler(dataRecord);
        }
        /// <summary>
        /// Creat build new method for new object.
        /// </summary>
        /// <param name="dataRecord"></param>
        /// <returns></returns>
        public static DataTableEntityBuilder<TEntity> CreateBuilder(DataRow dataRecord)
        {
            DataTableEntityBuilder<TEntity> dynamicBuilder = new DataTableEntityBuilder<TEntity>();
            DynamicMethod method = new DynamicMethod("DynamicCreateEntity", typeof(TEntity), new[] { typeof(DataRow) }, typeof(TEntity), true);
            ILGenerator generator = method.GetILGenerator();
            LocalBuilder result = generator.DeclareLocal(typeof(TEntity));
            // ReSharper disable once AssignNullToNotNullAttribute
            generator.Emit(OpCodes.Newobj, typeof(TEntity).GetConstructor(Type.EmptyTypes));
            generator.Emit(OpCodes.Stloc, result);

            for (int i = 0; i < dataRecord.ItemArray.Length; i++)
            {
                PropertyInfo propertyInfo = typeof(TEntity).GetProperty(dataRecord.Table.Columns[i].ColumnName);
                Label endIfLabel = generator.DefineLabel();
                if (propertyInfo?.GetSetMethod() != null)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldc_I4, i);
                    generator.Emit(OpCodes.Callvirt, IsDbNullMethod);
                    generator.Emit(OpCodes.Brtrue, endIfLabel);
                    generator.Emit(OpCodes.Ldloc, result);
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldc_I4, i);
                    generator.Emit(OpCodes.Callvirt, GetValueMethod);
                    generator.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
                    generator.Emit(OpCodes.Callvirt, propertyInfo.GetSetMethod());
                    generator.MarkLabel(endIfLabel);
                }
            }
            generator.Emit(OpCodes.Ldloc, result);
            generator.Emit(OpCodes.Ret);
            dynamicBuilder._handler = (Load)method.CreateDelegate(typeof(Load));
            return dynamicBuilder;
        }
    }
}
