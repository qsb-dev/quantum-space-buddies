using QSB.Messaging;
using QSB.Player.TransformSync;
using QSB.WorldSync;
using QSB.Player;

namespace QSB.Animation.Player.Messages;

public class PlayerHelmetMessage : QSBMessage<bool>
{
	static PlayerHelmetMessage()
	{
		GlobalMessenger.AddListener(OWEvents.PutOnHelmet, () => Handle(true));
		GlobalMessenger.AddListener(OWEvents.RemoveHelmet, () => Handle(false));
	}

	private static void Handle(bool on)
	{
		if (PlayerTransformSync.LocalInstance)
		{
			new PlayerHelmetMessage(on).Send();
		}
	}

	public PlayerHelmetMessage(bool on) : base(on) { }

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveRemote()
	{
		var player = QSBPlayerManager.GetPlayer(From);
		var animator = player.HelmetAnimator;
		if (Data)
		{
			animator.PutOnHelmet();
			player.AudioController.PlayWearHelmet();
		}
		else
		{
			animator.RemoveHelmet();
			player.AudioController.PlayRemoveHelmet();
		}
	}
}
