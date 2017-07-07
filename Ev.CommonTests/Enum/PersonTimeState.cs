/**============================================================
* 命名空间: Ev.CommonTests.Enum
*
* 功 能： N/A
* 类 名： PersonTimeState
*
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
* V0.01 2017/7/7 16:22:59 FreshMan 初版
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
    /// 人员状态
    /// </summary>
    public enum PersonTimeState
    {
        /// <summary>
        /// 工作
        /// </summary>
        [Description("工作")]
        Working = 0,

        /// <summary>
        /// 休息
        /// </summary>
        [Description("休息")]
        Off = 1,
    }
}
