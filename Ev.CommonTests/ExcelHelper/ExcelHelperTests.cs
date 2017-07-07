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
using System;
using System.Collections.Generic;
using Ev.Common.DataConvert;
using Ev.CommonTests.Enum;
using Ev.CommonTests.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace Ev.Common.ExcelHelper.Tests
{
    [TestClass]
    public class ExcelHelperTests
    {
        [TestMethod]
        public void DataTableToExcelTest()
        {
            List<TestsTabelToListObject> testList = new List<TestsTabelToListObject>
            {
                new TestsTabelToListObject
                {
                    Age = 10,
                    Height = 20.907,
                    Name = "qinxianbo",
                    Right = true,
                    Sex = EnumSex.Boy,
                    YouLong = new TimeSpan(1, 1, 1, 1),
                    BrityDay = new DateTime(2017, 2, 3)
                },
                new TestsTabelToListObject
                {
                    Age = 23,
                    Height = 234.907,
                    Name = "秦先波",
                    Right = true,
                    Sex = EnumSex.Boy,
                    YouLong = new TimeSpan(1, 1, 1, 2),
                    BrityDay = new DateTime(1994, 4, 5)
                },
                new TestsTabelToListObject
                {
                    Age = 40,
                    Height = 20.907,
                    Name = "qinxianbo",
                    Right = true,
                    Sex = EnumSex.Boy,
                    YouLong = new TimeSpan(1, 1, 1, 3),
                    BrityDay = new DateTime(2017, 2, 23)
                },
                new TestsTabelToListObject
                {
                    Height = 20.907,
                    Name = "杨宏俊",
                    Right = true,
                    Sex = EnumSex.Grily,
                    YouLong = new TimeSpan(1, 1, 1, 4),
                    BrityDay = new DateTime(1995, 6, 7)
                },
                new TestsTabelToListObject
                {
                    Age = 10,
                    Name = "k",
                    Height = 20.907,
                    Right = true,
                    Sex = EnumSex.Boy,
                    YouLong = new TimeSpan(1, 1, 1, 5)
                }
            };
            var table = DataTypeConvertHelper.ToDataTable(testList);
            var filePath = "..\\..\\TestUseFile\\DataTableToExcel.xlsx";
            var excelHelper = new ExcelHelper(filePath);
            var result = excelHelper.DataTableToExcel(table, "sheet", true);
            Assert.AreEqual(result - 1, testList.Count);
        }

        [TestMethod]
        public void ExcelToDataTableTest()
        {
            var filePath = "..\\..\\TestUseFile\\ExcelToDataTable.xls";
            var excelHelper = new ExcelHelper(filePath);
            var table = excelHelper.ExcelToDataTable(null, true);
            Assert.AreEqual(table.Rows.Count, 166);

            filePath = "..\\..\\TestUseFile\\TestExport.xlsx";
            var excelHelperTwo = new ExcelHelper(filePath);
            var tableTwo = excelHelperTwo.ExcelToDataTable(null, true);
            Assert.AreEqual(tableTwo.Rows.Count, 162);
        }

        [TestMethod]
        public void ReadExcelToDataSetTest()
        {
            var filePath = "..\\..\\TestUseFile\\BasicTestData.xlsx";
            var dataSet = new ExcelHelper(filePath).ReadExcelToDataSet();
            Assert.IsNotNull(dataSet);
            Assert.AreEqual(dataSet.Tables.Count, 13);

            filePath = "..\\..\\TestUseFile\\BasicTestData.xls";
            dataSet = new ExcelHelper(filePath).ReadExcelToDataSet();
            Assert.IsNotNull(dataSet);
            Assert.AreEqual(dataSet.Tables.Count, 2);
        }
    }
}