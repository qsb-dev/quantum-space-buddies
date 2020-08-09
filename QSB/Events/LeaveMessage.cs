using System;
using System.Linq;
using UnityEngine.Networking;

namespace QSB.Events
{
    public class LeaveMessage : PlayerMessage
    {
        public uint[] ObjectIds { get; set; }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            ObjectIds = reader.ReadString().Split(',').Select(x => Convert.ToUInt32(x)).ToArray();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(string.Join(",", ObjectIds.Select(x => x.ToString()).ToArray()));
        }
    }
}
