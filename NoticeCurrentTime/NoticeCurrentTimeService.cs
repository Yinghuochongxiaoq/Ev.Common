using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace NoticeCurrentTime
{
    public partial class NoticeCurrentTimeService : ServiceBase
    {
        readonly Timer _timer = new Timer();
        private const string LogFilePath = "d:\\history.txt";

        public NoticeCurrentTimeService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var elapsed = ConfigurationManager.AppSettings["elapsed"];

            _timer.Enabled = true;
            _timer.Interval = Convert.ToInt32(elapsed);
            _timer.AutoReset = true;
            _timer.Elapsed += NewThreadWriteDataToDb;
            FileStream fs = new FileStream(LogFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            StreamWriter sw = new StreamWriter(fs);
            sw.BaseStream.Seek(0, SeekOrigin.End);
            sw.WriteLine("NoticeCurrentTimeService:Service Started" + DateTime.Now + "\n");
            sw.Flush();
            sw.Close();
            fs.Close();
        }

        protected override void OnStop()
        {
            _timer.Enabled = false;
            FileStream fs = new FileStream(LogFilePath, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.BaseStream.Seek(0, SeekOrigin.End);
            sw.WriteLine("NoticeCurrentTimeService: Service Stopped" + DateTime.Now + "\n");
            sw.Flush();
            sw.Close();
            fs.Close();
        }

        private void NewThreadWriteDataToDb(object sender, ElapsedEventArgs e)
        {
            ConfigurationManager.RefreshSection("enable");
            ConfigurationManager.RefreshSection("elapsed");
            var enable = ConfigurationManager.AppSettings["enable"];
            var elapsed = ConfigurationManager.AppSettings["elapsed"];
            _timer.Interval = Convert.ToInt32(elapsed);
            if (!Convert.ToBoolean(enable)) return;
            Thread t = new Thread(WreiteDataToDb);
            t.Start();
        }

        private void WreiteDataToDb()
        {
            var connectionString = ConfigurationManager.AppSettings["connectionString"];
            if (string.IsNullOrEmpty(connectionString))
            {
                FileStream fs = new FileStream(LogFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                StreamWriter sw = new StreamWriter(fs);
                sw.BaseStream.Seek(0, SeekOrigin.End);
                sw.WriteLine("App.config file loss node :connectionString ,or connectionString is null " + DateTime.Now + "\n");
                sw.Flush();
                sw.Close();
                fs.Close();
                return;
            }
            const string safeSql = "INSERT INTO Log ([Date],[Thread],[Level],[Logger],[Message],[Exception]) VALUES (@log_date, @thread, @log_level, @logger, @message, @exception)";
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand(safeSql, connection);
                    SqlParameter[] values =
                    {
                        new SqlParameter("log_date", DateTime.Now),
                        new SqlParameter("thread", Process.GetCurrentProcess().Id),
                        new SqlParameter("log_level", "info"),
                        new SqlParameter("logger", "FreshMan"),
                        new SqlParameter("message", "message"),
                        new SqlParameter("exception", "")
                    };

                    if (values.Length > 0)
                    {
                        cmd.Parameters.AddRange(values);
                    }
                    cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                var exception =
                    new Exception(
                        $" Execute Sql command:{safeSql} maybe error,please check.Exception error message is:{ex.Message},innerExcetpion error message is :{ex.InnerException?.Message}",
                        ex);
                FileStream fs = new FileStream(LogFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                StreamWriter sw = new StreamWriter(fs);
                sw.BaseStream.Seek(0, SeekOrigin.End);
                sw.WriteLine(exception.Message);
                sw.Flush();
                sw.Close();
                fs.Close();
            }
        }
    }
}
