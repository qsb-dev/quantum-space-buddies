using QSB.WorldSync.Events;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.Anglerfish.Events
{
	public class AnglerChangeStateMessage : EnumWorldObjectMessage<AnglerfishController.AnglerState>
	{
		public uint TargetId;
		public Vector3 LocalDisturbancePos;

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			TargetId = reader.ReadUInt32();
			LocalDisturbancePos = reader.ReadVector3();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(TargetId);
			writer.Write(LocalDisturbancePos);
		}
	}
}
