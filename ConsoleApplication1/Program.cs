using System;
using System.Reflection;
using log4net;

namespace Ev.Common.WriteLog
{
    class Program
    {
        static void Main(string[] args)
        {
            //创建日志记录组件实例
            ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            //记录错误日志
            log.Error("error", new Exception("发生了一个异常"));
            //记录严重错误
            log.Fatal("fatal", new Exception("发生了一个致命错误"));
            //记录一般信息
            log.Info("info");
            //记录调试信息
            log.Debug("debug");
            //记录警告信息
            log.Warn("warn");
            Console.WriteLine("日志记录完毕。");
            Console.WriteLine("通过队列进行日志批量刷入，本机内存");

            Console.Read();
        }
    }
}
