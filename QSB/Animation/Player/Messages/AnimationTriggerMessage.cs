using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;
using QuantumUNET.Transport;

namespace QSB.Animation.Player.Messages
{
	internal class AnimationTriggerMessage : QSBMessage
	{
		private uint AttachedNetId;
		private string Name;

		public AnimationTriggerMessage(uint attachedNetId, string name)
		{
			AttachedNetId = attachedNetId;
			Name = name;
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(AttachedNetId);
			writer.Write(Name);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			AttachedNetId = reader.ReadUInt32();
			Name = reader.ReadString();
		}

		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;

		public override void OnReceiveRemote()
		{
			var animationSync = QSBPlayerManager.GetSyncObject<AnimationSync>(AttachedNetId);
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
