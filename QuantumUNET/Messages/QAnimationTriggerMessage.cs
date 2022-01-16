using QuantumUNET.Transport;

namespace QuantumUNET.Messages
{
	internal class QAnimationTriggerMessage : QMessageBase
	{
		public QNetworkInstanceId netId;
		public int hash;

		public override void Deserialize(QNetworkReader reader)
		{
			netId = reader.ReadNetworkId();
			hash = (int)reader.ReadPackedUInt32();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			writer.Write(netId);
			writer.WritePackedUInt32((uint)hash);
		}
	}
}