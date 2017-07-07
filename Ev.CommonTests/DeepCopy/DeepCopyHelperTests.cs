using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ev.Common.DeepCopy;
/**============================================================
* 命名空间: Ev.CommonTests.DeepCopy
*
* 功 能： N/A
* 类 名： DeepCopyHelperTests
*
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
* V0.01 2017/7/7 17:21:18 FreshMan 初版
*
* Copyright (c) 2017 Lir Corporation. All rights reserved.
*==============================================================
*==此技术信息为本公司机密信息,未经本公司书面同意禁止向第三方披露==
*==版权所有：重庆慧都科技有限公司                             ==
*==============================================================
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Ev.CommonTests.Enum;
using Ev.CommonTests.Model;

// ReSharper disable once CheckNamespace
namespace Ev.Common.DeepCopy.Tests
{
    [TestClass]
    public class DeepCopyHelperTests
    {
        [TestMethod]
        public void DeepCopyRecursionTest()
        {
            var tempObj = new TestsTabelToListObject
            {
                Age = 10,
                Name = "k",
                Height = 20.907,
                Right = true,
                Sex = EnumSex.Boy,
                YouLong = new TimeSpan(1, 1, 1, 5),
                AddressList = new List<string> { "FreshMan.com", "China.Chongqing" }
            };
            var copyResult = DeepCopyHelper.DeepCopyRecursion(tempObj) as TestsTabelToListObject;
            Debug.Assert(copyResult != null);
            Assert.AreEqual(new TimeSpan(1, 1, 1, 5), copyResult.YouLong);
            tempObj.AddressList[1] = "ChangeAddress";
            Assert.AreNotEqual(copyResult.AddressList[1], tempObj.AddressList[1]);
        }
    }
}