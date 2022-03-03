using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;

namespace QSB.Animation.Player.Messages;

internal class AnimationTriggerMessage : QSBMessage<string>
{
	public AnimationTriggerMessage(string name) => Data = name;

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveRemote()
	{
		var animationSync = QSBPlayerManager.GetPlayer(From).AnimationSync;
		if (animationSync == null)
		{
			return;
		}

		if (animationSync.VisibleAnimator == null)
		{
			return;
		}

		animationSync.VisibleAnimator.SetTrigger(Data);
	}
}