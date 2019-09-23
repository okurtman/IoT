using IOT.Domain.Server.Models.Delegates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IOT.Domain.Server
{
    public class IOTServer
    {
        private readonly TcpListener tcpListener;

        private Thread thread;

        private bool isListening = false;

        public event OnClientConnected OnClientConnected = null;

        public IOTServer()
        {
            tcpListener = new TcpListener(1234);
        }

        public void Start()
        {
            thread = new Thread(new ThreadStart(DoWork));
            thread.Start();

            tcpListener.Start();
        }

        public void Stop() 
        {
            try
            {
                isListening = false;
                thread.Join();

                tcpListener.Stop();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void DoWork()
        {
            isListening = true;

            while (isListening)
            {
                try
                {
                    if(tcpListener.Pending() == true)
                    {
                        var tcpClient = tcpListener.AcceptTcpClient();

                        if(OnClientConnected != null)
                        {
                            OnClientConnected.Invoke(tcpClient);
                        }
                    }
                }
                catch (Exception)
                {

                    throw;
                }

                Thread.Sleep(1000);
            }
        }
    }
}
