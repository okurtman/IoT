using IOT.Domain.Data.Enums;
using IOT.Domain.Data.Interfaces.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOT.Domain.Data.Models.Messages
{
    public sealed class Message : IMessage
    {
        public DateTime MessageTime { get; set; }

        public MessageType MessageType { get; set; }

        public ControllerType ControllerType { get; set; }

        public string Action { get; set; }

        public KeyValuePair<string, string>[] Args { get; set; }
    }
}
