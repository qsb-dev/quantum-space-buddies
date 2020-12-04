using UnityEngine.Networking;

namespace QSB.QuantumUNET
{
	internal class QSBObjectDestroyMessage : QSBMessageBase
	{
		public NetworkInstanceId NetId;

		public override void Deserialize(QSBNetworkReader reader)
		{
			NetId = reader.ReadNetworkId();
		}

		public override void Serialize(QSBNetworkWriter writer)
		{
			writer.Write(NetId);
		}
	}
}