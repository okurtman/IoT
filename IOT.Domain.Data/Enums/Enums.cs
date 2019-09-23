using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOT.Domain.Data.Enums
{
    public enum MessageType
    {
        Command,
        Info,
        Alert
    }

    public enum ControllerType
    {
        Led,
        Sensor
    }
}
