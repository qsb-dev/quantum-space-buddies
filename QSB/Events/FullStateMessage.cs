using System;
using System.Collections.Generic;
using System.Linq;
using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.Events
{
    class FullStateMessage : NameMessage
    {
        public override MessageType MessageType => MessageType.FullState;

        public Dictionary<uint, string> PlayerNames;

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            PlayerNames = new Dictionary<uint, string>();

            reader.ReadString().Split(',').ToList().ForEach(pair =>
            {
                var splitPair = pair.Split(':');
                var key = Convert.ToUInt16(splitPair[0]);
                var value = splitPair[1];
                PlayerNames[key] = value;
            });
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            var playerNamePairs = PlayerNames.ToList().Select(pair => $"{pair.Key}:{pair.Value}").ToArray();

            writer.Write(string.Join(",", playerNamePairs));
        }
    }
}
