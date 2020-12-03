using UnityEngine.Networking;

namespace QSB.QuantumUNET
{
	internal class QSBOwnerMessage : MessageBase
	{
		public NetworkInstanceId NetId;
		public short PlayerControllerId;

		public override void Deserialize(NetworkReader reader)
		{
			NetId = reader.ReadNetworkId();
			PlayerControllerId = (short)reader.ReadPackedUInt32();
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(NetId);
			writer.WritePackedUInt32((uint)PlayerControllerId);
		}
	}
}