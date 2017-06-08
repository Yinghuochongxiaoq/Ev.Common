/**============================================================
* 命名空间: Ev.CommonTests.Util
*
* 功 能： 枚举帮助类测试
* 类 名： EnumberHelperTests
*
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
* V0.01 2017/5/19 14:52:14 FreshMan 初版
*
* Copyright (c) 2017 Lir Corporation. All rights reserved.
*==============================================================
*==此技术信息为本公司机密信息,未经本公司书面同意禁止向第三方披露==
*==版权所有：重庆慧都科技有限公司                             ==
*==============================================================
*/

using Ev.Common.Enumber;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ev.CommonTests.Enumber
{
    [TestClass]
    public class EnumberHelperTests
    {
        /// <summary>
        /// 枚举转换成list集合测试
        /// </summary>
        [TestMethod]
        public void EnumToListTest()
        {
            var enumList = EnumberHelper.EnumToList<FreshTestEnum>();
            Assert.AreEqual(enumList.Count, 3);
        }

        /// <summary>
        /// 获取枚举对象的描述
        /// </summary>
        [TestMethod]
        public void GetEnumDescriptionTest()
        {
            var des = EnumberHelper.GetEnumDescription(FreshTestEnum.FreshMan);
            Assert.AreEqual(des, FreshTestEnum.FreshMan.ToString());
        }

        /// <summary>
        /// 根据枚举对象的值获取枚举对象
        /// </summary>
        [TestMethod]
        public void GetEnumByValueTest()
        {
            var enumTemp = EnumberHelper.GetEnumByValue<FreshTestEnum>(FreshTestEnum.ZhuBao.GetHashCode());
            Assert.AreEqual(enumTemp, FreshTestEnum.ZhuBao);
        }

        /// <summary>
        /// 根据枚举对象的名称获取枚举对象
        /// </summary>
        [TestMethod]
        public void GetEnumByNameTest()
        {
            var enumName = EnumberHelper.GetEnumByName<FreshTestEnum>(FreshTestEnum.FreshMan.ToString());
            Assert.AreEqual(enumName, FreshTestEnum.FreshMan);
        }
    }

    /// <summary>
    /// 测试枚举
    /// </summary>
    public enum FreshTestEnum
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,
        /// <summary>
        /// FreshMan
        /// </summary>
        FreshMan = 1,
        /// <summary>
        /// ZhuBao
        /// </summary>
        ZhuBao = 2
    }
}