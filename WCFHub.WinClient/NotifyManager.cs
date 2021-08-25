using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using WCFHub.IService;

namespace WCFHub.WinClient
{
    #region MessageReceive
    public delegate void MessageReceivedHandle(ArgumentBase<string> a);

    public class NotifyManager : IEventCallback
    {

        public event MessageReceivedHandle MessageReceived;

        public static int C_MaxErrCount = 5;
        public static int C_HeartbeatInterval = 1000 * 10;//10秒一次心跳检测
        IEventService _Proxy = null;
        private int ErrCounter = 0;

        public bool Enabled { get; set; }

        public NotifyManager()
        {
            Enabled = false;
        }

        private void Close()
        {
            if (_Proxy != null)
            {
                try
                {
                    var comObj = _Proxy as ICommunicationObject;
                    comObj.Abort();
                }
                catch { }
            }
        }
        public void Start()
        {
            Enabled = true;
            StartInternal();
            #region 心跳检测
            var timer = new System.Timers.Timer();
            timer.Enabled = false;
            timer.Interval = C_HeartbeatInterval;
            timer.Elapsed += (s, ie) =>
            {
                try
                {
                    WriteLine("心跳检测...");
                    timer.Enabled = false;
                    _Proxy.Ping();
                    ErrCounter = 0;
                }
                catch (Exception ex)
                {
                    WriteLine(ex.Message);

                    ErrCounter++;
                    if (ErrCounter >= C_MaxErrCount)
                    {
                        Close();
                        StartInternal();
                    }
                }
                finally
                {
                    timer.Enabled = true;
                }
            };
            timer.Start();
            #endregion
        }

        private void StartInternal()
        {
            if (!Enabled) return;

            lock (this)
            {

                try
                {
                    #region
                    ErrCounter = 0;

                    _Proxy = WCFHelper.Factory.CreateChannel(new InstanceContext(this));



                    var comObj = _Proxy as ICommunicationObject;

                    comObj.Faulted += (s, ie) =>
                    {
                        WriteLine("Faulted");

                    };
                    comObj.Closed += (s, ie) =>
                    {
                        WriteLine("Closed!");
                    };
                    comObj.Closing += (s, ie) =>
                    {
                        WriteLine("Closing!");
                    };


                    WriteLine("加载并配置完成!");

                    _Proxy.Subscribe(new SubscribeArg() { Username = Guid.NewGuid().ToString("N") });
                    WriteLine("注册成功!");
                    #endregion

                }
                catch (Exception ex)
                {

                    WriteLine(ex.Message);

                }
            }
        }

        public void Stop()
        {
            Enabled = false;
            Close();
        }

        public void WriteLine(string msg)
        {
            Console.WriteLine(msg + "," + DateTime.Now);
        }

        #region IEventCallback 成员

        public void OnMessageReceived(ArgumentBase<string> a)
        {
            if (MessageReceived != null)
            {
                MessageReceived(a);
            }
        }

        #endregion
    }
    #endregion
}
