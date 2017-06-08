/**============================================================
* 命名空间: Ev.CommonTests.Util
*
* 功 能： ExcelHelper测试类
* 类 名： ExcelHelperTests
*
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
* V0.01 2017/5/19 14:39:46 FreshMan 初版
*
* Copyright (c) 2017 Lir Corporation. All rights reserved.
*==============================================================
*==此技术信息为本公司机密信息,未经本公司书面同意禁止向第三方披露==
*==版权所有：重庆慧都科技有限公司                             ==
*==============================================================
*/

using Ev.Common.ExcelHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ev.CommonTests.Util
{
    [TestClass]
    public class ExcelHelperTests
    {
        /// <summary>
        /// 测试文件
        /// </summary>
        [TestMethod]
        public void ReadExcelToDataSetTest()
        {
            var filePath = "BasicTestData.xlsx";
            var dataSet = ExcelHelper.ReadExcelToDataSet(filePath);
            Assert.IsNotNull(dataSet);
            Assert.AreEqual(dataSet.Tables.Count,13);
            filePath = "BasicTestData.xls";
            dataSet = ExcelHelper.ReadExcelToDataSet(filePath);
            Assert.IsNotNull(dataSet);
            Assert.AreEqual(dataSet.Tables.Count,2);
        }
    }
}