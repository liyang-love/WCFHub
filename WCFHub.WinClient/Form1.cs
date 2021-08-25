using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WCFHub.IService;

namespace WCFHub.WinClient
{
    public partial class Form1 : Form
    {
        private SynchronizationContext SyncContext = null;
        public Form1()
        {
            InitializeComponent();
            SyncContext = SynchronizationContext.Current;
        }

        NotifyManager _NotifyManager = null;


        private void Form1_Load(object sender, EventArgs e)
        {

            _NotifyManager = new NotifyManager();
            _NotifyManager.MessageReceived += OnMessageReceived;
            _NotifyManager.Start();
        }
        protected override void OnClosed(EventArgs e)
        {
            if (_NotifyManager != null)
            {
                _NotifyManager.MessageReceived -= this.OnMessageReceived;
                _NotifyManager.Stop();
            }
            base.OnClosed(e);
        }

        public void OnMessageReceived(ArgumentBase<string> a)
        {
            Console.WriteLine("收到消息:" + a.Model + ",InvokeRequired:" + this.InvokeRequired);

            if (this.InvokeRequired)
            {
                SyncContext.Post((d) =>
                {
                    textBox1.Text += a.Model + Environment.NewLine;
                }, null);
            }
            else
            {
                textBox1.Text += a.Model + Environment.NewLine;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
