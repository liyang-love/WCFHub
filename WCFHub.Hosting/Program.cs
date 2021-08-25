using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace WCFHub.Hosting
{
    class Program
    {
        static void Main(string[] args)
        {

            using (ServiceHost host = new ServiceHost(typeof(HISBusinessCache)))
            {
                try
                {
                    host.Open();
                    Console.WriteLine("启动时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    Console.WriteLine("---------------------------------------------");
                    Console.WriteLine("服务启动 ... ...");
                   
                }
                finally
                {
                    host.Close();
                }
            }
        }
    }
}
