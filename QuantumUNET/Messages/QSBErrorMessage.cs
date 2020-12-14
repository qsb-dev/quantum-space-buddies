namespace QuantumUNET.Messages
{
	public class QSBErrorMessage : QSBMessageBase
	{
		public override void Deserialize(QSBNetworkReader reader) => errorCode = reader.ReadUInt16();

		public override void Serialize(QSBNetworkWriter writer) => writer.Write((ushort)errorCode);

		public int errorCode;
	}
}