using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Windows.Forms;
using WCFHub.IService;

namespace WCFHub.Win
{
    public partial class Form1 : Form
    {
        private ServiceHost _Host = new ServiceHost(typeof(EventServiceImpl));

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _Host.AddServiceEndpoint(typeof(IEventService), new NetTcpBinding(SecurityMode.None),
                                     "net.tcp://127.0.0.1:9999/EventService"
                                      );
            _Host.Open();
            Console.WriteLine("服务开启...");
        }


        protected override void OnClosed(EventArgs e)
        {
            _Host.Close();
            base.OnClosed(e);
            Console.WriteLine("服务关闭!");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var data = new ArgumentBase<string>() { Model = textBox1.Text + "," + DateTime.Now.ToString() };
            EventServiceImpl.PostData(data);
        }
    }
}
