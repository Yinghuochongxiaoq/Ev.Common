using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace NoticeCurrentTime
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            FileStream fs=new FileStream(@"d:\xx.txt",FileMode.OpenOrCreate,FileAccess.ReadWrite);
            StreamWriter sw=new StreamWriter(fs);
            sw.BaseStream.Seek(0, SeekOrigin.End);
            sw.WriteLine("WindowsService:Service Started" + DateTime.Now+"\n");
            sw.Flush();
            sw.Close();
            fs.Close();
        }

        protected override void OnStop()
        {
            FileStream fs = new FileStream(@"d:\xx.txt", FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.BaseStream.Seek(0, SeekOrigin.End);
            sw.WriteLine("WindowsService: Service Stopped" + DateTime.Now + "\n");
            sw.Flush();
            sw.Close();
            fs.Close();
        }
    }
}
