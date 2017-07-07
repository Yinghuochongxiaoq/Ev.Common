/**============================================================
* 命名空间: Ev.CommonTests.Model
*
* 功 能： N/A
* 类 名： TestsTabelToListObject
*
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
* V0.01 2017/7/7 16:44:19 FreshMan 初版
*
* Copyright (c) 2017 Lir Corporation. All rights reserved.
*==============================================================
*==此技术信息为本公司机密信息,未经本公司书面同意禁止向第三方披露==
*==版权所有：重庆慧都科技有限公司                             ==
*==============================================================
*/
using System;
using System.Collections.Generic;
using Ev.CommonTests.Enum;

namespace Ev.CommonTests.Model
{
    public class TestsTabelToListObject
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        public int Age { get; set; }

        public double Height { get; set; }

        public EnumSex Sex { get; set; }

        public TimeSpan YouLong { get; set; }

        public bool Right { get; set; }

        public List<string> AddressList { get; set; }

        public DateTime BrityDay { get; set; }
    }
}
