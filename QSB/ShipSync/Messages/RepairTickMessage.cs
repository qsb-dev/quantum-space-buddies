using QSB.Messaging;
using QSB.WorldSync;
using QuantumUNET.Transport;

namespace QSB.ShipSync.Messages
{
	internal abstract class RepairTickMessage<T> : QSBWorldObjectMessage<T> where T : IWorldObject
	{
		protected float RepairFraction;

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(RepairFraction);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			RepairFraction = reader.ReadSingle();
		}
	}
}
