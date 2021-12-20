using QuantumUNET.Transport;

namespace QuantumUNET.Messages
{
	internal class QAnimationMessage : QMessageBase
	{
		public QNetworkInstanceId netId;
		public int stateHash;
		public float normalizedTime;
		public byte[] parameters;

		public override void Serialize(QNetworkWriter writer)
		{
			writer.Write(netId);
			writer.WritePackedUInt32((uint)stateHash);
			writer.Write(normalizedTime);
			writer.WriteBytesAndSize(parameters, parameters?.Length ?? 0);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			netId = reader.ReadNetworkId();
			stateHash = (int)reader.ReadPackedUInt32();
			normalizedTime = reader.ReadSingle();
			parameters = reader.ReadBytesAndSize();
		}
	}
}