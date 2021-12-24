using QSB.WorldSync.Events;
using QuantumUNET.Transport;

namespace QSB.QuantumSync.Messages
{
	public class MultiStateChangeMessage : WorldObjectMessage
	{
		public int StateIndex { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			StateIndex = reader.ReadInt32();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(StateIndex);
		}
	}
}