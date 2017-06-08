/**============================================================
* 命名空间: Ev.Common.Util
*
* 功 能： 枚举描述实体
* 类 名： EnumberEntity
*
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
* V0.01 2017/5/19 11:58:14 FreshMan 初版
*
* Copyright (c) 2017 Lir Corporation. All rights reserved.
*==============================================================
*==此技术信息为本公司机密信息,未经本公司书面同意禁止向第三方披露==
*==版权所有：重庆慧都科技有限公司                             ==
*==============================================================
*/

namespace Ev.Common.Enumber
{
    /// <summary>
    /// 枚举描述实体
    /// </summary>
    public sealed class EnumberEntity
    {
        /// <summary>
        /// 枚举描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 枚举名称
        /// </summary>
        public string EnumName { get; set; }

        /// <summary>
        /// 枚举对象的值
        /// </summary>
        public int EnumValue { get; set; }
    }
}
