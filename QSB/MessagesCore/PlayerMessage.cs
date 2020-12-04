using QSB.QuantumUNET;
using UnityEngine.Networking;

namespace QSB.Messaging
{
	public class PlayerMessage : QSBMessageBase
	{
		public uint FromId { get; set; }
		public uint AboutId { get; set; }

		public override void Deserialize(QSBNetworkReader reader)
		{
			FromId = reader.ReadUInt32();
			AboutId = reader.ReadUInt32();
		}

		public override void Serialize(QSBNetworkWriter writer)
		{
			writer.Write(FromId);
			writer.Write(AboutId);
		}
	}
}