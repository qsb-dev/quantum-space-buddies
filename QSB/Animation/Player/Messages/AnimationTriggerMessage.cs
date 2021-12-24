using QSB.Messaging;
using QuantumUNET.Transport;

namespace QSB.Animation.Player.Messages
{
	public class AnimationTriggerMessage : PlayerMessage
	{
		public uint AttachedNetId { get; set; }
		public string Name { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			AttachedNetId = reader.ReadUInt32();
			Name = reader.ReadString();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(AttachedNetId);
			writer.Write(Name);
		}
	}
}
