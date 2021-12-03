using QuantumUNET.Transport;

namespace QuantumUNET.Messages
{
	internal class QOwnerMessage : QMessageBase
	{
		public NetworkInstanceId NetId;
		public short PlayerControllerId;

		public override void Serialize(QNetworkWriter writer)
		{
			writer.Write(NetId);
			writer.WritePackedUInt32((uint)PlayerControllerId);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			NetId = reader.ReadNetworkId();
			PlayerControllerId = (short)reader.ReadPackedUInt32();
		}
	}
}