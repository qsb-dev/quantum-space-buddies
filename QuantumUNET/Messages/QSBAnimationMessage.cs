namespace QuantumUNET.Messages
{
	internal class QSBAnimationMessage : QSBMessageBase
	{
		public QSBNetworkInstanceId netId;
		public int stateHash;
		public float normalizedTime;
		public byte[] parameters;

		public override void Deserialize(QSBNetworkReader reader)
		{
			netId = reader.ReadNetworkId();
			stateHash = (int)reader.ReadPackedUInt32();
			normalizedTime = reader.ReadSingle();
			parameters = reader.ReadBytesAndSize();
		}

		public override void Serialize(QSBNetworkWriter writer)
		{
			writer.Write(netId);
			writer.WritePackedUInt32((uint)stateHash);
			writer.Write(normalizedTime);
			if (parameters == null)
			{
				writer.WriteBytesAndSize(parameters, 0);
			}
			else
			{
				writer.WriteBytesAndSize(parameters, parameters.Length);
			}
		}
	}
}