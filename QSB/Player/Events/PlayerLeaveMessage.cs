using QSB.Messaging;
using QSB.QuantumUNET;
using System;
using System.Linq;

namespace QSB.Player.Events
{
	public class PlayerLeaveMessage : PlayerMessage
	{
		public uint[] NetIds { get; set; }

		public override void Deserialize(QSBNetworkReader reader)
		{
			base.Deserialize(reader);
			NetIds = reader.ReadString().Split(',').Select(x => Convert.ToUInt32(x)).ToArray();
		}

		public override void Serialize(QSBNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(string.Join(",", NetIds.Select(x => x.ToString()).ToArray()));
		}
	}
}