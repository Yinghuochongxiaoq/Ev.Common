/**============================================================
* 命名空间: Common.Common.Providers
*
* 功 能： N/A
* 类 名： AuthProvider
*
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
* V0.01 2017/5/15 10:19:08 FreshMan 初版
*
* Copyright (c) 2017 Lir Corporation. All rights reserved.
*==============================================================
*==此技术信息为本公司机密信息,未经本公司书面同意禁止向第三方披露==
*==版权所有：重庆慧都科技有限公司                             ==
*==============================================================
*/

using System;

namespace Ev.Common.Providers
{
    /// <summary>
    /// Auth Provider
    /// </summary>
    public abstract class AuthProvider
    {
        private static AuthProvider _instance;

        /// <summary>
        /// This method determines whether the user is authorize to perform the requested operation
        /// </summary>
        /// <author>FreshMan</author>
        /// <creattime>2017-05-16</creattime>
        public abstract bool CheckAccess(object operation);

        /// <summary>
        /// No params init provider
        /// </summary>
        /// <author>FreshMan</author>
        /// <creattime>2017-05-16</creattime>
        /// <typeparam name="TProvider"></typeparam>
        public static void Initialize<TProvider>() where TProvider : AuthProvider, new()
        {
            _instance = new TProvider();
        }

        /// <summary>
        /// params init provider
        /// </summary>
        /// <author>FreshMan</author>
        /// <creattime>2017-05-16</creattime>
        /// <typeparam name="TProvider"></typeparam>
        /// <param name="parameters"></param>
        public static void Initialize<TProvider>(Enum[] parameters)
        {
            var constructorInfo = typeof(TProvider).GetConstructor(new[] { typeof(object[]) });
            if (constructorInfo != null) _instance = (AuthProvider)constructorInfo.Invoke(new object[] { parameters });
        }

        /// <summary>
        /// instance get_method
        /// </summary>
        /// <author>FreshMan</author>
        /// <creattime>2017-05-16</creattime>
        public static AuthProvider Instance => _instance;
    }
}
