using QSB.QuantumUNET;
using UnityEngine.Networking;

namespace QSB.Animation.Events
{
	internal class QSBAnimationParametersMessage : QSBMessageBase
	{
		public NetworkInstanceId netId;
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