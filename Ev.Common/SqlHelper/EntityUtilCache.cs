/**============================================================
* 命名空间: Ev.Common.SqlHelper
*
* 功 能： N/A
* 类 名： EntityUtilCache
*
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
* V0.01 2017/10/16 15:00:21 FreshMan 初版
*
* Copyright (c) 2017 Lir Corporation. All rights reserved.
*==============================================================
*==此技术信息为本公司机密信息,未经本公司书面同意禁止向第三方披露==
*==版权所有：重庆慧都科技有限公司                             ==
*==============================================================
*/

using System.ComponentModel;

namespace Ev.Common.SqlHelper
{
    /// <summary>
    /// entity util cache class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal static class EntityUtilCache<T>
    {
        /// <summary>
        /// 使用委托
        /// </summary>
        internal static readonly SetPropertyValueInvoker<T> SetPropertyInvoker;

        /// <summary>
        /// 使用委托
        /// </summary>
        internal static readonly GetPropertyValueInvoker<T> GetPropertyInvoker;

        /// <summary>
        /// 构造函数
        /// </summary>
        static EntityUtilCache()
        {
            SetPropertyInvoker = SetPropertyModel<T>.SetPropertyValue;
            GetPropertyInvoker = GetPropertyModel<T>.GetPropertyValue;
        }
    }

}
