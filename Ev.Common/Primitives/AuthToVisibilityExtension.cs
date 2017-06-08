/**============================================================
* 命名空间: Common.Common.Primitives
*
* 功 能： N/A
* 类 名： AuthToVisibilityExtension
*
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
* V0.01 2017/5/15 10:17:38 FreshMan 初版
*
* Copyright (c) 2017 Lir Corporation. All rights reserved.
*==============================================================
*==此技术信息为本公司机密信息,未经本公司书面同意禁止向第三方披露==
*==版权所有：重庆慧都科技有限公司                             ==
*==============================================================
*/

using System;
using System.Windows.Markup;
using Ev.Common.Providers;
using System.Windows;

namespace Ev.Common.Primitives
{
    /// <summary>
    /// 是否显示标记扩展
    /// </summary>
    [MarkupExtensionReturnType(typeof(Visibility))]
    public class AuthToVisibilityExtension : MarkupExtension
    {
        /// <summary>
        /// 权限枚举
        /// </summary>
        /// <author>FreshMan</author>
        /// <creattime>2017-05-15</creattime>
        public Enum Operation;

        /// <summary>
        /// 无参数构造函数
        /// </summary>
        /// <author>FreshMan</author>
        /// <creattime>2017-05-15</creattime>
        public AuthToVisibilityExtension()
        {
        }

        /// <summary>
        /// 有参数构造函数
        /// </summary>
        /// <author>FreshMan</author>
        /// <creattime>2017-05-15</creattime>
        /// <param name="permission">权限枚举</param>
        public AuthToVisibilityExtension(Enum permission)
        {
            Operation = permission;
        }

        /// <summary>
        /// 重写ProvideValue方法，返回实例的期望的属性
        /// </summary>
        /// <author>FreshMan</author>
        /// <creattime>2017-05-15</creattime>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Operation==null) return Visibility.Collapsed;

            if (AuthProvider.Instance.CheckAccess(Operation)) return Visibility.Visible;
            return Visibility.Hidden;
        }
    }
}
