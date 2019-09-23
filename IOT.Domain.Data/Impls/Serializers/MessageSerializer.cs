using IOT.Domain.Data.Enums;
using IOT.Domain.Data.Interfaces.Messages;
using IOT.Domain.Data.Models.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOT.Domain.Data.Impls.Serializers
{
    public class MessageSerializer
    {
        static Dictionary<KeyValuePair<MessageType, ControllerType>, Type> messageTypes = new Dictionary<KeyValuePair<MessageType, ControllerType>, Type>
        {
            {
                new KeyValuePair<MessageType, ControllerType>(LedOnOffMessage.MESSAGE_TYPE, LedOnOffMessage.CONTROLLER_TYPE),
                typeof(LedOnOffMessage)
            },
            {
                new KeyValuePair<MessageType, ControllerType>(SensorInfoMessage.MESSAGE_TYPE, SensorInfoMessage.CONTROLLER_TYPE),
                typeof(SensorInfoMessage)
            }
        };

        static Dictionary<char, Action<Message, string[]>> Splitters = new Dictionary<char, Action<Message, string[]>>
        {
            {
                '/',
                (message, args) =>
                {
                    message.MessageType = (MessageType)Enum.Parse(typeof(MessageType), args[0]);
                    message.ControllerType = (ControllerType)Enum.Parse(typeof(ControllerType), args[1]);
                }
            },
            {
                '?',
                (message, args) =>
                {
                    message.Action = args[0];
                }
            },
            {
                '&',
                (message, args) =>
                {
                    message.Args = args
                    .Select(q => {
                        var argArgs = q.Split('=');
                        return new KeyValuePair<string, string>(argArgs[0], argArgs[1]);
                    })
                    .ToArray();
                }
            }
        };

        public static IMessage Deserialize(string str)
        {
            Message message = new Message();

            foreach (var splitter in Splitters)
            {
                var args = str.Split(splitter.Key);

                splitter.Value.Invoke(message, args);

                str = args[args.Length - 1];
            }

            Type t = null;

            foreach (var messageType in messageTypes)
            {
                if (message.MessageType == messageType.Key.Key && message.ControllerType == messageType.Key.Value)
                {
                    t = messageType.Value;
                    break;
                }
            }

            if (t == typeof(LedOnOffMessage))
            {
                return new LedOnOffMessage(message.Action, message.Args);
            }
            else if (t == typeof(SensorInfoMessage))
            {
                return new SensorInfoMessage(message.Action, message.Args);
            }

            return message;
        }

        public static string Serialize(IMessage message)
        {
            return string.Empty;
        }
    }
}
