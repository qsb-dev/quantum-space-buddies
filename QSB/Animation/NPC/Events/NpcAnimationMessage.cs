using QSB.Messaging;
using QuantumUNET.Transport;

namespace QSB.Animation.NPC.Events
{
	class NpcAnimationMessage : PlayerMessage
	{
		public AnimationEvent AnimationEvent { get; set; }
		public int AnimControllerIndex { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			AnimationEvent = (AnimationEvent)reader.ReadInt32();
			AnimControllerIndex = reader.ReadInt32();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)AnimationEvent);
			writer.Write(AnimControllerIndex);
		}
	}
}
