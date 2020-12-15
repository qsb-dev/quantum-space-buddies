namespace QuantumUNET.Messages
{
	internal class QSBObjectDestroyMessage : QSBMessageBase
	{
		public QSBNetworkInstanceId NetId;

		public override void Deserialize(QSBNetworkReader reader) => NetId = reader.ReadNetworkId();

		public override void Serialize(QSBNetworkWriter writer) => writer.Write(NetId);
	}
}