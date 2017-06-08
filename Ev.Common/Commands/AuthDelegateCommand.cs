/**============================================================
* 命名空间: Common.Common.Commands
*
* 功 能： N/A
* 类 名： AuthDelegateCommand
*
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
* V0.01 2017/5/16 16:54:37 FreshMan 初版
*
* Copyright (c) 2017 Lir Corporation. All rights reserved.
*==============================================================
*==此技术信息为本公司机密信息,未经本公司书面同意禁止向第三方披露==
*==版权所有：重庆慧都科技有限公司                             ==
*==============================================================
*/

using System;
using Common.Common.Commands;
using Ev.Common.Providers;

namespace Ev.Common.Commands
{
    /// <summary>
    /// 权限控制命令
    /// </summary>
    public class AuthDelegateCommand : DelegateCommandBase
    {
        /// <summary>
        /// 构造函数命令
        /// </summary>
        /// <param name="executeMethod"></param>
        public AuthDelegateCommand(Action executeMethod)
            : base(op => executeMethod(), op => AuthProvider.Instance.CheckAccess(op))
        {
            if (executeMethod == null)
                throw new ArgumentNullException(nameof(executeMethod));
        }

        /// <summary>
        /// 重写 execute
        /// </summary>
        public void Execute()
        {
            base.Execute(null);
        }
    }

    /// <summary>
    /// 带参数的命令类
    /// </summary>
    public class AuthDelegateCommand<T> : DelegateCommandBase
    {
        /// <summary>
        /// 带参数构造函数
        /// </summary>
        /// <param name="executeMethod">执行方法</param>
        public AuthDelegateCommand(Action<T> executeMethod)
            : base(op => executeMethod((T)op), op => AuthProvider.Instance.CheckAccess(op))
        {
            if (executeMethod == null)
                throw new ArgumentNullException(nameof(executeMethod));

            Type genericType = typeof(T);
            if (genericType.IsValueType)
            {
                if ((!genericType.IsGenericType) || (!typeof(Nullable<>).IsAssignableFrom(genericType.GetGenericTypeDefinition())))
                {
                    throw new InvalidCastException("T for AuthDelegateCommand<T> is not an object nor Nullable.");
                }
            }
        }

        /// <summary>
        /// 重写 canexecute
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(T parameter)
        {
            return base.CanExecute(parameter);
        }

        /// <summary>
        /// 重写 execute
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(T parameter)
        {
            base.Execute(parameter);
        }
    }
}
