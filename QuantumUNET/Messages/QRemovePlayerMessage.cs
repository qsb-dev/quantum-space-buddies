using QuantumUNET.Transport;

namespace QuantumUNET.Messages
{
	public class QRemovePlayerMessage : QMessageBase
	{
		public short PlayerControllerId;

		public override void Serialize(QNetworkWriter writer) => writer.Write((ushort)PlayerControllerId);

		public override void Deserialize(QNetworkReader reader) => PlayerControllerId = (short)reader.ReadUInt16();
	}
}