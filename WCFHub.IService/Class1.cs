using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

namespace WCFHub.IService
{
    [ServiceContract(CallbackContract = typeof(IEventCallback))]
    public interface IEventService
    {
        [OperationContract(IsOneWay = true)]
        void Subscribe(SubscribeArg a);

        [OperationContract(IsOneWay = true)]
        void Unsubscribe(ArgumentBase<String> a);

        [OperationContract]
        DateTime Ping();

    }

    public interface IEventCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnMessageReceived(ArgumentBase<String> a);
    }

    public class EventServiceImpl : IEventService
    {
        public static readonly ConcurrentDictionary<String, SubscribeContext> _Subscribers = new ConcurrentDictionary<String, SubscribeContext>();

        public string ClientIpAndPort()
        {
            OperationContext context = OperationContext.Current;
            MessageProperties properties = context.IncomingMessageProperties;
            RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
            return endpoint.Address + ":" + endpoint.Port.ToString();
        }

        public void Subscribe(SubscribeArg a)
        {
            Console.WriteLine(ClientIpAndPort());

            var callback = OperationContext.Current.GetCallbackChannel<IEventCallback>();
            a.Username = a.Username.ToLower();
            _Subscribers[a.Username] = new SubscribeContext() { Arg = a, Callback = callback };

            #region 事件处理
            ICommunicationObject obj = (ICommunicationObject)callback;
            obj.Closed += (s, e) =>
            {

                Console.WriteLine("Closed");
            };

            obj.Faulted += (s, e) => {

                Console.WriteLine("Faulted");
            };

            obj.Closing += (s, e) =>
            {

                Console.WriteLine("Closeing" + OperationContext.Current);

                var callback2 = (IEventCallback)s;

                _Subscribers.ToList().ForEach(ent => {

                    if (ent.Value.Callback == callback2)
                    {
                        RemoveSubscriber(ent.Value.Arg.Username);
                    }
                });
            };
            #endregion

        }

        public void Unsubscribe(ArgumentBase<string> a)
        {
            RemoveSubscriber(a.Model);

        }
        private static void RemoveSubscriber(string username)
        {
            username = username.ToLower();
            if (_Subscribers.ContainsKey(username))
            {
                SubscribeContext outObj = null;
                _Subscribers.TryRemove(username, out outObj);
            }
        }

        public static void PostData(ArgumentBase<string> a)
        {
            Console.WriteLine("收到待发消息:" + a.Model);

            _Subscribers.ToList().ForEach(subscriber =>
            {


                ICommunicationObject callback = (ICommunicationObject)subscriber.Value.Callback;
                if (((ICommunicationObject)callback).State == CommunicationState.Opened)
                {
                    try
                    {
                        //此处需要加上权限判断、订阅判断等
                        subscriber.Value.Callback.OnMessageReceived(a);
                    }
                    catch (Exception ex)
                    {
                        RemoveSubscriber(subscriber.Value.Arg.Username);
                        Console.WriteLine("PostData:" + ex.Message);
                    }
                }
                else
                {
                    RemoveSubscriber(subscriber.Value.Arg.Username);
                    Console.WriteLine("PostData,用户链接已经关闭");
                }



            });
        }


        #region IEventService 成员


        public DateTime Ping()
        {
            Console.WriteLine("Ping:" + ClientIpAndPort() + "," + DateTime.Now);
            return DateTime.Now;
        }

        #endregion
    }
    #region Model

    public class SubscribeContext
    {
        public SubscribeArg Arg { get; set; }
        public IEventCallback Callback { get; set; }

    }


    [Serializable]
    public class ArgumentBase<T>
    {
        private int code;
        private string msg;
        private T model;

        public int Code
        {
            get { return code; }
            set { code = value; }
        }
        public string Msg
        {
            get { return msg; }
            set { msg = value; }
        }
        public T Model
        {
            get { return model; }
            set { model = value; }

        }
    }

    public class SubscribeArg : ArgumentBase<int>
    {
        public String Username { get; set; }
        public List<int> Alarms { get; set; }
        public SubscribeArg()
        {
            Alarms = new List<int>();
        }
    } 
    #endregion
}
