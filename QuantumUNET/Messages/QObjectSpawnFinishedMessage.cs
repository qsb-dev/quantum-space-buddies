using QuantumUNET.Transport;

namespace QuantumUNET.Messages
{
	internal class QObjectSpawnFinishedMessage : QMessageBase
	{
		public uint State;

		public override void Serialize(QNetworkWriter writer) => writer.WritePackedUInt32(State);

		public override void Deserialize(QNetworkReader reader) => State = reader.ReadPackedUInt32();
	}
}