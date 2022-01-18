using Mirror;
using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;

namespace QSB.Animation.Player.Messages
{
	internal class AnimationTriggerMessage : QSBMessage
	{
		private uint PlayerId;
		private string Name;

		public AnimationTriggerMessage(uint playerId, string name)
		{
			PlayerId = playerId;
			Name = name;
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(PlayerId);
			writer.Write(Name);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			PlayerId = reader.Read<uint>();
			Name = reader.ReadString();
		}

		public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

		public override void OnReceiveRemote()
		{
			var animationSync = QSBPlayerManager.GetPlayer(PlayerId).AnimationSync;
			if (animationSync == null)
			{
				return;
			}

			if (animationSync.VisibleAnimator == null)
			{
				return;
			}

			animationSync.VisibleAnimator.SetTrigger(Name);
		}
	}
}
