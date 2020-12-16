using QuantumUNET.Transport;

namespace QuantumUNET.Messages
{
	internal class QSBAnimationParametersMessage : QSBMessageBase
	{
		public QSBNetworkInstanceId netId;
		public byte[] parameters;

		public override void Deserialize(QSBNetworkReader reader)
		{
			netId = reader.ReadNetworkId();
			parameters = reader.ReadBytesAndSize();
		}

		public override void Serialize(QSBNetworkWriter writer)
		{
			writer.Write(netId);
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