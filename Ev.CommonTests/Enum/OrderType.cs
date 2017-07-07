/**============================================================
* 命名空间: Ev.CommonTests.Enum
*
* 功 能： N/A
* 类 名： OrderType
*
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
* V0.01 2017/7/7 16:20:29 FreshMan 初版
*
* Copyright (c) 2017 Lir Corporation. All rights reserved.
*==============================================================
*==此技术信息为本公司机密信息,未经本公司书面同意禁止向第三方披露==
*==版权所有：重庆慧都科技有限公司                             ==
*==============================================================
*/

using System.ComponentModel;

namespace Ev.CommonTests.Enum
{
    /// <summary>
    /// 订单类型
    /// </summary>
    public enum OrderType
    {
        /// <summary>
        /// 
        /// </summary>
        [Description("Other")]
        Other = 0,

        /// <summary>
        /// 
        /// </summary>
        [Description("SO")]
        SO = 1,
        /// <summary>
        /// 
        /// </summary>
        [Description("WO")]
        WO = 2,

        /// <summary>
        /// 
        /// </summary>
        [Description("PlanOrderHdrM")]
        PlanOrderHdrM = 4,

        /// <summary>
        /// 
        /// </summary>
        [Description("Detail")]
        WoDet = 8
    }
}
