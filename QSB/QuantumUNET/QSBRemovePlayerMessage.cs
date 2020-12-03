using UnityEngine.Networking;

namespace QSB.QuantumUNET
{
	public class QSBRemovePlayerMessage : MessageBase
	{
		public override void Deserialize(NetworkReader reader)
		{
			this.playerControllerId = (short)reader.ReadUInt16();
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write((ushort)this.playerControllerId);
		}

		public short playerControllerId;
	}
}