using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IOT.Domain.Client
{
    public class IOTClient
    {
        private readonly TcpClient tcpClient;

        private Thread readThread;
        private Thread writeThread;

        private Queue<string> messageQueue = new Queue<string>();

        public delegate void Connected(IOTClient client);
        public delegate void Disconnected(IOTClient client);

        public delegate void MessageReceived(IOTClient client, string message);

        public event Connected OnConnected = null;
        public event Disconnected OnDisconnected = null;
        public event MessageReceived OnMessageReceived = null;

        public IOTClient(int id, string host, int port)
        {
            ID = id;
            Host = host;
            Port = port;

            tcpClient = new TcpClient();
        }

        public IOTClient(TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
        }

        public void Start()
        {
            readThread = new Thread(new ThreadStart(DoRead));
            readThread.Start();

            writeThread = new Thread(new ThreadStart(DoWrite));
            writeThread.Start();
        }

        private void DoWrite()
        {
            while (tcpClient.Connected)
            {
                try
                {
                    var stream = tcpClient.GetStream();

                    if (messageQueue.Count > 0)
                    {
                        if (stream.CanWrite)
                        {
                            var message = messageQueue.Dequeue() + "\r\n";

                            var buffer = ASCIIEncoding.UTF8.GetBytes(message);

                            stream.Write(buffer, 0, buffer.Length);
                        }

                        //Thread.Sleep(500);

                        //if (stream.CanRead)
                        //{
                        //    var buffer = new byte[256];

                        //    var bytesRead = stream.Read(buffer, 0, buffer.Length);
                        //    if (bytesRead > 0)
                        //    {
                        //        var message = ASCIIEncoding.UTF8.GetString(buffer, 0, bytesRead);

                        //        Console.WriteLine("Message received from server");
                        //        Console.WriteLine(message);
                        //    }
                        //}
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occured while writing ex: {ex.Message}");
                }
                finally
                {
                    Thread.Sleep(100);
                }
            }
        }

        private void DoRead()
        {
            while (tcpClient.Connected)
            {
                try
                {
                    var stream = tcpClient.GetStream();

                    if (stream.CanRead)
                    {
                        var buffer = new byte[256];

                        var bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            var message = ASCIIEncoding.UTF8.GetString(buffer, 0, bytesRead);

                            if (OnMessageReceived != null)
                            {
                                OnMessageReceived.Invoke(this, message);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occured while reading ex: {ex.Message}");
                }
                finally
                {
                    Thread.Sleep(100);
                }
            }

            if(OnDisconnected != null)
            {
                OnDisconnected.Invoke(this);
            }
        }

        public void Send(string message)
        {
            messageQueue.Enqueue(message);
        }

        public string Host { get; private set; }

        public int Port { get; private set; }

        public int ID { get; private set; }

        public bool Connect()
        {
            Console.WriteLine($"IOTClient:{ID} connecting to {Host}:{Port}");

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    tcpClient.Connect(Host, Port);
                    if (tcpClient.Connected)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An exception occured while IOTClient:{ID} is connecting to {Host}:{Port}");
                    Console.WriteLine(ex.Message);
                }
            }

            if (tcpClient.Connected)
            {
                Console.WriteLine($"IOTClient:{ID} connected to {Host}:{Port}");

                readThread = new Thread(new ThreadStart(DoWork));
                readThread.Start();
            }

            return tcpClient.Connected;
        }

        public void Disconnect()
        {
            if (tcpClient.Connected)
            {
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        tcpClient.Close();
                        if(tcpClient.Connected == false)
                        {
                            Console.WriteLine($"IOTClient:{ID} disconnected from {Host}:{Port}");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An exception occured while IOTClient:{ID} is disconnecting from {Host}:{Port}");
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            readThread.Join();
        }

        private void DoWork()
        {
            while (tcpClient.Connected)
            {
                try
                {
                    var stream = tcpClient.GetStream();
                    if (stream.CanWrite)
                    {
                        var random = new Random();

                        var temp = random.Next(0, 100);
                        var hum = (double)random.Next(40, 60) / 100;

                        var message = $"Info/Sensor/State?no=1&temp={temp}&hum={hum}&water_level=0&soil_humidity=50&total_mili_litres=10&relay_status=0";

                        var buffer = Encoding.UTF8.GetBytes(message);

                        stream.Write(buffer, 0, buffer.Length);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    Thread.Sleep(3000);
                }
            }
        }
    }
}
