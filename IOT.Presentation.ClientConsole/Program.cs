using IOT.Domain.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOT.Presentation.ClientConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new IOTClient(1, "localhost", 1234);

            client.Connect();

            while (true)
            {
                var cmd = Console.ReadLine();
                if (cmd.Equals("exit"))
                {
                    client.Disconnect();
                    break;
                }
            }
        }
    }
}
