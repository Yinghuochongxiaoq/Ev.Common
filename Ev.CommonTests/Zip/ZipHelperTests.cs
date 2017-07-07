using Microsoft.VisualStudio.TestTools.UnitTesting;
/**============================================================
* 命名空间: Ev.CommonTests.Zip
*
* 功 能： N/A
* 类 名： ZipHelperTests
*
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
* V0.01 2017/7/7 17:54:28 FreshMan 初版
*
* Copyright (c) 2017 Lir Corporation. All rights reserved.
*==============================================================
*==此技术信息为本公司机密信息,未经本公司书面同意禁止向第三方披露==
*==版权所有：重庆慧都科技有限公司                             ==
*==============================================================
*/

// ReSharper disable once CheckNamespace
namespace Ev.Common.Zip.Tests
{
    [TestClass]
    public class ZipHelperTests
    {
        [TestMethod]
        public void GZipCompressStringTest()
        {
            var str = "1";
            var enCode = ZipHelper.GZipCompressString(str);
            var deCode = ZipHelper.GZipDecompressString(enCode);
            Assert.AreEqual(str, deCode);
        }

        [TestMethod]
        public void GZipDecompressStringTest()
        {
            var str = "1djifaniJHUNj&94850#$@^pdnfmcf478451578`~)(（）+- _)(%@! {}[]【】";
            var enCode = ZipHelper.GZipCompressString(str);
            var deCode = ZipHelper.GZipDecompressString(enCode);
            Assert.AreEqual(str, deCode);
        }
    }
}