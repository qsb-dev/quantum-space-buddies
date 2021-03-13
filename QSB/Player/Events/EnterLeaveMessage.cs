using QSB.WorldSync.Events;
using QuantumUNET.Transport;

namespace QSB.Player.Events
{
	class EnterLeaveMessage : WorldObjectMessage
	{
		public EnterLeaveType Type { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			Type = (EnterLeaveType)reader.ReadInt32();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)Type);
		}
	}
}
