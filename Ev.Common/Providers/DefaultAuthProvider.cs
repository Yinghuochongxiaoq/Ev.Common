/**============================================================
* 命名空间: Common.Common.Providers
*
* 功 能： 默认权限处理类
* 类 名： DefaultAuthProvider
*
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
* V0.01 2017/5/15 10:19:24 FreshMan 初版
*
* Copyright (c) 2017 Lir Corporation. All rights reserved.
*==============================================================
*==此技术信息为本公司机密信息,未经本公司书面同意禁止向第三方披露==
*==版权所有：重庆慧都科技有限公司                             ==
*==============================================================
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Ev.Common.Providers
{
    /// <summary>
    /// 默认权限处理类
    /// </summary>
    /// <author>FreshMan</author>
    /// <creattime>2017-05-16</creattime>
    public class DefaultAuthProvider : AuthProvider
    {
        /// <summary>
        /// 枚举集合
        /// </summary>
        private static List<Enum> _operations;

        /// <summary>
        /// 无参构造函数
        /// </summary>
        /// <author>FreshMan</author>
        /// <creattime>2017-05-16</creattime>
        public DefaultAuthProvider() { }

        /// <summary>
        /// 设置可用权限枚举集合
        /// </summary>
        /// <author>FreshMan</author>
        /// <creattime>2017-05-16</creattime>
        public DefaultAuthProvider(object[] parameters)
        {
            _operations = parameters?.Cast<Enum>().ToList();
        }

        /// <summary>
        /// 通过枚举权项检测权限
        /// </summary>
        /// <author>FreshMan</author>
        /// <creattime>2017-05-16</creattime>
        /// <returns>检测是否通过</returns>
        public override bool CheckAccess(object operation)
        {
            operation = operation as Enum;
            if (operation == null) return false;
            if (Equals(operation.GetHashCode(), 0)) return true;
            if (_operations != null && _operations.Count > 0)
            {
                //TODO 是否存在一个可用权限项
                return _operations.Any(p => Equals(p, operation));
            }
            return operation.GetHashCode() == 0;
        }
    }
}
