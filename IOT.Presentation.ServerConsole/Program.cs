using IOT.Domain.Client;
using IOT.Domain.Data.Impls.Serializers;
using IOT.Domain.Data.Interfaces.Messages;
using IOT.Domain.Data.Models.Messages;
using IOT.Domain.Server;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IOT.Presentation.ServerConsole
{
    class Program
    {
        static IOTHub hub = new IOTHub();

        static void Main(string[] args)
        {

            Thread.CurrentThread.CurrentCulture = new CultureInfo(ConfigurationManager.AppSettings["lang"]);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(ConfigurationManager.AppSettings["lang"]);

            string url = "http://localhost:1235";

            using (WebApp.Start(url))
            {
                Console.WriteLine("Server running on {0}", url);

                var server = new IOTServer();

                server.OnClientConnected += Server_OnClientConnected;

                server.Start();

                while (true)
                {
                    var cmd = Console.ReadLine();
                    if (cmd.Equals("exit"))
                    {
                        server.Stop();
                        break;
                    }
                    else
                    {
                        //if (hub.ClientList.Count > 0)
                        //{
                        //    hub.ClientList[0].Send(cmd);
                        //}
                    }
                }
            }

        }

        private static void Server_OnClientConnected(TcpClient tcpClient)
        {
            Console.WriteLine("IOTClient connected");

            var iotClient = new IOTClient(tcpClient);

            iotClient.OnDisconnected += IotClient_OnDisconnected;
            iotClient.OnMessageReceived += IotClient_OnMessageReceived;

            hub.OnIotConnected(iotClient);

            iotClient.Start();
        }

        private static void IotClient_OnDisconnected(IOTClient client)
        {
            client.OnDisconnected -= IotClient_OnDisconnected;
            client.OnMessageReceived -= IotClient_OnMessageReceived;

            hub.OnIotDisconnected(client);

            Console.WriteLine("IOTClient disconnected");
        }

        private static void IotClient_OnMessageReceived(IOTClient sender, string message)
        {
            Console.WriteLine("Message received from client");
            Console.WriteLine(message);

            hub.OnIotMessageReceived(message);
        }
    }

    public class IOTClientOnMessageReceivedArgs
    {
        public IMessage Message { get; set; }

        public IOTClient Client { get; set; }
    }

    class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //app.UseCors(CorsOptions.AllowAll);
            //app.MapSignalR();

            // Branch the pipeline here for requests that start with "/signalr"
            app.Map("/signalr", map =>
            {
                // Setup the CORS middleware to run before SignalR.
                // By default this will allow all origins. You can 
                // configure the set of origins and/or http verbs by
                // providing a cors options with a different policy.
                map.UseCors(CorsOptions.AllowAll);
                var hubConfiguration = new HubConfiguration
                {
                    // You can enable JSONP by uncommenting line below.
                    // JSONP requests are insecure but some older browsers (and some
                    // versions of IE) require JSONP to work cross domain
                    EnableJSONP = true
                };
                // Run the SignalR pipeline. We're not using MapSignalR
                // since this branch already runs under the "/signalr"
                // path.

                hubConfiguration.EnableDetailedErrors = true;
                map.RunSignalR(hubConfiguration);
            });
        }
    }

    [HubName("iotHub")]
    public class IOTHub : Hub
    {
        static IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<IOTHub>();

        static List<IOTClient> ClientList = new List<IOTClient>();

        public IOTHub()
        {

        }

        public void Send(string str)
        {
            //var message = MessageSerializer.Deserialize(str);

            //Console.WriteLine(JsonConvert.SerializeObject(message));

            if(ClientList.Count > 0)
            {
                ClientList[0].Send(str);
            }

            Clients.All.addMessage(str);
        }

        public List<SensorInfoMessage> GetDailyReport()
        {
            var endTime = DateTime.Now;
            var startTime = endTime.AddDays(-1);

            var endFileName = endTime.ToString("yyyy-MM-dd") + ".txt";
            var startFileName = startTime.ToString("yyyy-MM-dd") + ".txt";

            var fileList = new List<string> { startFileName, endFileName };

            var dailyData = new List<SensorInfoMessage>();

            foreach (var fileName in fileList)
            {
                if (File.Exists(fileName))
                {
                    using (var fs = File.OpenRead(fileName))
                    {
                        using (var sr = new StreamReader(fs))
                        {
                            var lineString = sr.ReadLine();

                            while (!string.IsNullOrEmpty(lineString))
                            {
                                var args = lineString.Split(' ');

                                var messageTime = DateTime.ParseExact(args[0], "yyyyMMddHHmmss", CultureInfo.InvariantCulture);

                                if (messageTime >= startTime || messageTime <= endTime)
                                {
                                    var message = MessageSerializer.Deserialize(args[1]);

                                    message.MessageTime = messageTime;

                                    if (message is SensorInfoMessage)
                                    {
                                        dailyData.Add(message as SensorInfoMessage);
                                    }
                                }

                                lineString = sr.ReadLine();
                            }
                        }
                    }
                }
            }

            return dailyData;
        }

        public void OnIotConnected(IOTClient client)
        {
            ClientList.Add(client);

            hubContext.Clients.All.onConnected();
        }

        public void OnIotDisconnected(IOTClient client)
        {
            ClientList.Remove(client);

            hubContext.Clients.All.onDisconnected();
        }

        public void OnIotMessageReceived(string str)
        {
            var message = MessageSerializer.Deserialize(str);

            if(message is SensorInfoMessage)
            {
                var sensorInfoMessage = message as SensorInfoMessage;

                var fileName = sensorInfoMessage.MessageTime.ToString("yyyy-MM-dd") + ".txt";

                using (var sw = File.AppendText(fileName))
                {
                    var lineString = string.Format("{0:yyyyMMddHHmmss} {1}", sensorInfoMessage.MessageTime, str);

                    sw.WriteLine(lineString);
                }
            }

            Console.WriteLine(JsonConvert.SerializeObject(message));

            hubContext.Clients.All.onMessageReceived(message);
        }
    }
}
