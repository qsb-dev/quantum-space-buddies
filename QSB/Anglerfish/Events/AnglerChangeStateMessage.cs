using QSB.WorldSync.Events;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.Anglerfish.Events
{
	public class AnglerChangeStateMessage : EnumWorldObjectMessage<AnglerfishController.AnglerState>
	{
		public uint targetId;
		public Vector3 localDisturbancePos;

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			targetId = reader.ReadUInt32();
			localDisturbancePos = reader.ReadVector3();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(targetId);
			writer.Write(localDisturbancePos);
		}
	}
}
