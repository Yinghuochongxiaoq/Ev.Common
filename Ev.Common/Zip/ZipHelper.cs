/**============================================================
* 命名空间: Ev.Common.Zip
*
* 功 能： N/A
* 类 名： ZipHelper
*
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
* V0.01 2017/7/7 17:39:15 FreshMan 初版
*
* Copyright (c) 2017 Lir Corporation. All rights reserved.
*==============================================================
*==此技术信息为本公司机密信息,未经本公司书面同意禁止向第三方披露==
*==版权所有：重庆慧都科技有限公司                             ==
*==============================================================
*/

using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Ev.Common.Zip
{
    /// <summary>
    /// Zip helper
    /// </summary>
    public class ZipHelper
    {
        /// <summary>
        /// 将传入字符串以GZip算法压缩后，返回Base64编码字符  
        /// </summary>
        /// <param name="rawString">需要压缩的字符串</param>
        /// <returns>压缩后的Base64编码的字符串</returns>
        public static string GZipCompressString(string rawString)
        {
            if (string.IsNullOrEmpty(rawString) || rawString.Length == 0)
            {
                return "";
            }
            else
            {
                byte[] rawData = Encoding.UTF8.GetBytes(rawString);
                byte[] zippedData = Compress(rawData);
                return Convert.ToBase64String(zippedData);
            }

        }

        /// <summary>  
        /// 将传入的二进制字符串资料以GZip算法解压缩  
        /// </summary>  
        /// <param name="zippedString">经GZip压缩后的二进制字符串</param>  
        /// <returns>原始未压缩字符串</returns>  
        public static string GZipDecompressString(string zippedString)
        {
            if (string.IsNullOrEmpty(zippedString) || zippedString.Length == 0)
            {
                return "";
            }
            else
            {
                byte[] zippedData = Convert.FromBase64String(zippedString);
                return Encoding.UTF8.GetString(Decompress(zippedData));
            }
        }

        /// <summary>
        /// GZip压缩
        /// </summary>
        /// <param name="rawData"></param>  
        /// <returns></returns>
        public static byte[] Compress(byte[] rawData)
        {
            MemoryStream ms = new MemoryStream();
            GZipStream compressedzipStream = new GZipStream(ms, CompressionMode.Compress, true);
            compressedzipStream.Write(rawData, 0, rawData.Length);
            compressedzipStream.Close();
            return ms.ToArray();
        }

        /// <summary>
        /// ZIP解压
        /// </summary>
        /// <param name="zippedData"></param>  
        /// <returns></returns>
        public static byte[] Decompress(byte[] zippedData)
        {
            MemoryStream ms = new MemoryStream(zippedData);
            GZipStream compressedzipStream = new GZipStream(ms, CompressionMode.Decompress);
            MemoryStream outBuffer = new MemoryStream();
            byte[] block = new byte[1024];
            while (true)
            {
                int bytesRead = compressedzipStream.Read(block, 0, block.Length);
                if (bytesRead <= 0)
                    break;
                else
                    outBuffer.Write(block, 0, bytesRead);
            }
            compressedzipStream.Close();
            return outBuffer.ToArray();
        }
    }
}
