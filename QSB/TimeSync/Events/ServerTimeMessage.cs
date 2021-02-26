using QSB.Messaging;
using QuantumUNET.Transport;

namespace QSB.TimeSync.Events
{
	public class ServerTimeMessage : PlayerMessage
	{
		public float ServerTime { get; set; }
		public int LoopCount { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			ServerTime = reader.ReadSingle();
			LoopCount = reader.ReadInt16();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(ServerTime);
			writer.Write(LoopCount);
		}
	}
}