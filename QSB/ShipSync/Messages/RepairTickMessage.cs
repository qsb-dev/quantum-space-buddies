using QSB.WorldSync.Events;
using QuantumUNET.Transport;

namespace QSB.ShipSync.Messages
{
	internal class RepairTickMessage : WorldObjectMessage
	{
		public float RepairNumber;

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(RepairNumber);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			RepairNumber = reader.ReadSingle();
		}
	}
}
