using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Networking;

namespace QSB.Messaging
{
    public class PlayerLeaveMessage : PlayerMessage
    {
        public NetworkInstanceId[] NetIds { get; set; }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            NetIds = DeserializeFromString<NetworkInstanceId[]>(reader.ReadString());
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(SerializeToString(NetIds));
        }

        public static string SerializeToString<T>(T value)
        {
            using (var stream = new MemoryStream())
            {
                (new BinaryFormatter()).Serialize(stream, value);
                stream.Flush();
                return Convert.ToBase64String(stream.ToArray());
            }
        }
        public static T DeserializeFromString<T>(string data)
        {
            byte[] bytes = Convert.FromBase64String(data);
            using (var stream = new MemoryStream(bytes))
                return (T)(new BinaryFormatter()).Deserialize(stream);
        }
    }
}