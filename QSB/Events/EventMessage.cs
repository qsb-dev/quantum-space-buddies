using QSB.Messaging;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Networking;

namespace QSB.Events
{
    public class EventMessage : PlayerMessage
    {
        public override MessageType MessageType => MessageType.Event;

        public int EventType { get; set; }
        public object[] Data { get; set; }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            EventType = reader.ReadInt32();
            var guff = reader.ReadString();
            var strings = guff.Split('|');
            var temp = new List<object>();
            foreach (var item in strings)
            {
                temp.Add(StringToObject(item));
            }
            Data = temp.ToArray();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(EventType);
            var data = "";
            foreach (var item in Data)
            {
                data += "|" + ObjectToString(item);
            }
            data = data.TrimStart('|');
            writer.Write(data);
        }

        public string ObjectToString(object obj)
        {
            using (var ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, obj);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public object StringToObject(string base64String)
        {
            byte[] bytes = Convert.FromBase64String(base64String);
            using (var ms = new MemoryStream(bytes, 0, bytes.Length))
            {
                ms.Write(bytes, 0, bytes.Length);
                ms.Position = 0;
                return new BinaryFormatter().Deserialize(ms);
            }
        }
    }
}
