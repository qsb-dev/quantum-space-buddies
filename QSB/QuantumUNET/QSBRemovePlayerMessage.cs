using UnityEngine.Networking;

namespace QSB.QuantumUNET
{
	public class QSBRemovePlayerMessage : MessageBase
	{
		public short PlayerControllerId;

		public override void Deserialize(NetworkReader reader)
		{
			PlayerControllerId = (short)reader.ReadUInt16();
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write((ushort)PlayerControllerId);
		}
	}
}