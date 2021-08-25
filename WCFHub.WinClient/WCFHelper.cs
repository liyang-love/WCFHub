using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using WCFHub.IService;

namespace WCFHub.WinClient
{
    public class WCFHelper
    {

        private static DuplexChannelFactory<IEventService> _channelFac;

        public static DuplexChannelFactory<IEventService> Factory
        {
            get
            {
                if (_channelFac == null)
                {
                    _channelFac =
                        new DuplexChannelFactory<IEventService>(typeof(NotifyManager), new NetTcpBinding(SecurityMode.None),
                        EndpointStr);

                }
                return _channelFac;
            }
        }

        private static string EndpointStr
        {
            get
            {
                return "net.tcp://127.0.0.1:9999/EventService";
            }
        }
    }
}
