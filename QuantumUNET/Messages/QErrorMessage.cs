using QuantumUNET.Transport;

namespace QuantumUNET.Messages
{
	public class QErrorMessage : QMessageBase
	{
		public int errorCode;

		public override void Serialize(QNetworkWriter writer) => writer.Write((ushort)errorCode);

		public override void Deserialize(QNetworkReader reader) => errorCode = reader.ReadUInt16();
	}
}