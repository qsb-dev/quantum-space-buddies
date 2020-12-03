using UnityEngine.Networking;

namespace QSB.QuantumUNET
{
	internal class QSBObjectSpawnFinishedMessage : MessageBase
	{
		public uint State;

		public override void Deserialize(NetworkReader reader)
		{
			State = reader.ReadPackedUInt32();
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.WritePackedUInt32(State);
		}
	}
}