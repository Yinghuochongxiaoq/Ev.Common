/**============================================================
* 命名空间: Common.Common.Primitives
*
* 功 能： N/A
* 类 名： AuthToEnabledExtension
*
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
* V0.01 2017/5/15 10:17:18 FreshMan 初版
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

namespace Ev.Common.Primitives
{
    /// <summary>
    /// 是否可用扩展标记
    /// </summary>
    /// <author>FreshMan</author>
    /// <creattime>2017-05-15</creattime>
    [MarkupExtensionReturnType(typeof(bool))]
    public class AuthToEnabledExtension : MarkupExtension
    {
        /// <summary>
        /// 权限选项
        /// </summary>
        public Enum Operation { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <author>FreshMan</author>
        /// <creattime>2017-05-15</creattime>
        public AuthToEnabledExtension()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <author>FreshMan</author>
        /// <creattime>2017-05-15</creattime>
        /// <param name="operation"></param>
        public AuthToEnabledExtension(Enum operation)
        {
            Operation = operation;
        }

        /// <summary>
        /// 判定是否可用
        /// </summary>
        /// <author>FreshMan</author>
        /// <creattime>2017-05-15</creattime>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Operation == null) return false;
            return AuthProvider.Instance.CheckAccess(Operation);
        }
    }
}
