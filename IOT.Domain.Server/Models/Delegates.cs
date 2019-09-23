using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IOT.Domain.Server.Models.Delegates
{
    public delegate void OnClientConnected(TcpClient tcpClient);
}
