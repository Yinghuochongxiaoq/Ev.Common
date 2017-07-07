/**============================================================
* 命名空间: Ev.CommonTests.DataConvert
*
* 功 能： N/A
* 类 名： DataTypeConvertHelperTests
*
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
* V0.01 2017/7/7 16:13:31 FreshMan 初版
*
* Copyright (c) 2017 Lir Corporation. All rights reserved.
*==============================================================
*==此技术信息为本公司机密信息,未经本公司书面同意禁止向第三方披露==
*==版权所有：重庆慧都科技有限公司                             ==
*==============================================================
*/
using System.Collections.Generic;
using Ev.CommonTests.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace Ev.Common.DataConvert.Tests
{
    [TestClass]
    public class DataTypeConvertHelperTests
    {
        /// <summary>
        /// list to datatable slowly
        /// </summary>
        [TestMethod]
        public void ToDataTableSlowlyTest()
        {
            var filePath = "..\\..\\TestUseFile\\TestExportLessData.xlsx";
            var excelHelperTwo = new ExcelHelper.ExcelHelper(filePath);
            var tableTwo = excelHelperTwo.ExcelToDataTable(null, true);
            List<PersonGattScheduleInfoDto> personGantScheduleInfoDtosList =
                DataTypeConvertHelper.ToList<PersonGattScheduleInfoDto>(tableTwo);
            var storgeTable = DataTypeConvertHelper.ToDataTable(personGantScheduleInfoDtosList);
            Assert.AreEqual(tableTwo.Rows.Count, storgeTable.Rows.Count);
            var i = 5;
            var j = 4;
            Assert.AreEqual(tableTwo.Rows[i][j], storgeTable.Rows[i][j]);
        }

        /// <summary>
        /// list to datatable fast
        /// </summary>
        [TestMethod]
        public void ToDataTableTest()
        {
            var filePath = "..\\..\\TestUseFile\\TestExport.xlsx";
            var excelHelperTwo = new ExcelHelper.ExcelHelper(filePath);
            var tableTwo = excelHelperTwo.ExcelToDataTable(null, true);
            List<PersonGattScheduleInfoDto> personGantScheduleInfoDtosList =
                DataTypeConvertHelper.ToList<PersonGattScheduleInfoDto>(tableTwo);
            var storgeTable = DataTypeConvertHelper.ToDataTable(personGantScheduleInfoDtosList);
            Assert.AreEqual(tableTwo.Rows.Count, storgeTable.Rows.Count);
            var i = 5;
            var j = 4;
            Assert.AreEqual(tableTwo.Rows[i][j], storgeTable.Rows[i][j]);
        }

        /// <summary>
        /// DataTable to list
        /// </summary>
        [TestMethod]
        public void ToListTest()
        {
            var filePath = "..\\..\\TestUseFile\\TestExport.xlsx";
            var excelHelperTwo = new ExcelHelper.ExcelHelper(filePath);
            var tableTwo = excelHelperTwo.ExcelToDataTable(null, true);
            List<PersonGattScheduleInfoDto> tableToList =
                DataTypeConvertHelper.ToList<PersonGattScheduleInfoDto>(tableTwo);
            Assert.AreEqual(tableTwo.Rows.Count, tableToList.Count);
        }

        [TestMethod]
        public void ToIntTest()
        {
            var intNumber = "90";
            var intNumberResult = DataTypeConvertHelper.ToInt(intNumber);
            Assert.AreEqual(intNumberResult, 90);

            var strNoNumber = "90你好";
            var convertNumberIsZero = DataTypeConvertHelper.ToInt(strNoNumber);
            Assert.AreEqual(convertNumberIsZero, 0);
        }

        [TestMethod]
        public void ToIntTest1()
        {
            var intNumber = "90";
            var intNumberResult = DataTypeConvertHelper.ToInt(intNumber,20);
            Assert.AreEqual(intNumberResult, 90);

            var strNoNumber = "90你好";
            var convertNumberIsDefault = DataTypeConvertHelper.ToInt(strNoNumber,20);
            Assert.AreEqual(convertNumberIsDefault, 20);
        }
    }
}