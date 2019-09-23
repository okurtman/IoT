using IOT.Domain.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOT.Domain.Data.Interfaces.Messages
{
    public interface IMessage
    {
        DateTime MessageTime { get; set; }

        MessageType MessageType { get; }

        ControllerType ControllerType { get; }

        string Action { get; }

        KeyValuePair<string, string>[] Args { get; }
    }
}
