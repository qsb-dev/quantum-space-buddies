using QSB.WorldSync.Events;
using QuantumUNET.Transport;

namespace QSB.QuantumSync.Events
{
	internal class MultiStateChangeMessag : WorldObjectMessage
	{
		public int StateIndex { get; set; }

		public override void Deserialize(QSBNetworkReader reader)
		{
			base.Deserialize(reader);
			StateIndex = reader.ReadInt32();
		}

		public override void Serialize(QSBNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(StateIndex);
		}
	}
}