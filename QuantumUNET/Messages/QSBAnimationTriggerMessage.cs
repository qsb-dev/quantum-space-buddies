namespace QuantumUNET.Messages
{
	internal class QSBAnimationTriggerMessage : QSBMessageBase
	{
		public QSBNetworkInstanceId netId;
		public int hash;

		public override void Deserialize(QSBNetworkReader reader)
		{
			netId = reader.ReadNetworkId();
			hash = (int)reader.ReadPackedUInt32();
		}

		public override void Serialize(QSBNetworkWriter writer)
		{
			writer.Write(netId);
			writer.WritePackedUInt32((uint)hash);
		}
	}
}