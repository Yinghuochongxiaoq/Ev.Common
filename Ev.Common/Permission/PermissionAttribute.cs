/**============================================================
* 命名空间: Common.Common.Models
*
* 功 能： 用户权限特性。
* 类 名： SystemPowerRightAttribute
*
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
* V0.01 2017/5/11 16:10:55 FreshMan 初版
*
* Copyright (c) 2017 Lir Corporation. All rights reserved.
*==============================================================
*==此技术信息为本公司机密信息,未经本公司书面同意禁止向第三方披露==
*==版权所有：重庆慧都科技有限公司                             ==
*==============================================================
*/

using System;

namespace Ev.Common.Permission
{
    /// <summary>
    /// 用户权限
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class PermissionAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="powerEnum">权限枚举，在最终检测是转换成枚举使用</param>
        public PermissionAttribute(object powerEnum)
        {
            CurrentPowerEnum = powerEnum;
        }

        /// <summary>
        /// 只读属性获取当前类或者方法的权限枚举
        /// </summary>
        public object CurrentPowerEnum { get; }
    }
}
