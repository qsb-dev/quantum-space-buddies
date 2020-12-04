using UnityEngine.Networking;

namespace QSB.QuantumUNET
{
	public class QSBRemovePlayerMessage : QSBMessageBase
	{
		public short PlayerControllerId;

		public override void Deserialize(QSBNetworkReader reader)
		{
			PlayerControllerId = (short)reader.ReadUInt16();
		}

		public override void Serialize(QSBNetworkWriter writer)
		{
			writer.Write((ushort)PlayerControllerId);
		}
	}
}