/**============================================================
* 命名空间: Ev.Common.Util
*
* 功 能： N/A
* 类 名： EnumberHelper
*
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
* V0.01 2017/5/19 11:57:32 FreshMan 初版
*
* Copyright (c) 2017 Lir Corporation. All rights reserved.
*==============================================================
*==此技术信息为本公司机密信息,未经本公司书面同意禁止向第三方披露==
*==版权所有：重庆慧都科技有限公司                             ==
*==============================================================
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Ev.Common.Enumber
{
    /// <summary>
    /// 枚举类型帮助类
    /// </summary>
    public class EnumberHelper
    {
        /// <summary>
        /// 将枚举类型转换成list集合
        /// </summary>
        /// <author>FreshMan</author>
        /// <creattime>2017-05-19</creattime>
        /// <returns>枚举list集合</returns>
        public static List<EnumberEntity> EnumToList<T>()
        {
            List<EnumberEntity> list = new List<EnumberEntity>();
            foreach (var e in Enum.GetValues(typeof(T)))
            {
                var m = new EnumberEntity();
                object[] objArr = e.GetType()
                    .GetField(e.ToString())
                    .GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (objArr.Length > 0)
                {
                    DescriptionAttribute description = objArr[0] as DescriptionAttribute;
                    m.Description = description?.Description;
                }
                m.EnumValue = Convert.ToInt32(e);
                m.EnumName = e.ToString();
                list.Add(m);
            }
            return list;
        }

        /// <summary>
        /// 获得枚举值的Description特性的值，一般是消息的搜索码
        /// </summary>
        /// <author>FreshMan</author>
        /// <creattime>2017-05-19</creattime>
        /// <returns>返回查找到的Description特性的值，如果没有，就返回.ToString()</returns>
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            if (fi == null) return value.ToString();
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
        }

        /// <summary>
        /// 通过枚举值获得枚举值
        /// </summary>
        /// <author>FreshMan</author>
        /// <creattime>2017-05-19</creattime>
        /// <returns>枚举对象</returns>
        public static T GetEnumByValue<T>(int enumValue) where T : struct
        {
            if (enumValue < 0) return default(T);
            return (T)Enum.Parse(typeof(T), enumValue.ToString());
        }

        /// <summary>
        /// 通过枚举名称获得枚举值
        /// </summary>
        /// <author>FreshMan</author>
        /// <creattime>2017-05-19</creattime>
        /// <returns>枚举对象</returns>
        public static T GetEnumByName<T>(string enumName) where T : struct
        {
            if (string.IsNullOrEmpty(enumName)) return default(T);
            return (T)Enum.Parse(typeof(T), enumName);
        }
    }
}
