using QSB.Messaging;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.WorldSync;

namespace QSB.Animation.Player.Messages;

public class PlayerSuitMessage : QSBMessage<bool>
{
	static PlayerSuitMessage()
	{
		GlobalMessenger.AddListener(OWEvents.SuitUp, () => Handle(true));
		GlobalMessenger.AddListener(OWEvents.RemoveSuit, () => Handle(false));
	}

	private static void Handle(bool on)
	{
		if (PlayerTransformSync.LocalInstance)
		{
			new PlayerSuitMessage(on).Send();
		}
	}

	public PlayerSuitMessage(bool on) : base(on) { }

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveRemote()
	{
		var player = QSBPlayerManager.GetPlayer(From);
		player.SuitedUp = Data;

		if (!player.IsReady)
		{
			return;
		}

		var animator = player.AnimationSync;
		animator.SetSuitState(Data);

		if (player.SuitedUp)
		{
			player.AudioController.PlayWearSuit();
		}
		else
		{
			player.AudioController.PlayRemoveSuit();
		}
	}

	public override void OnReceiveLocal()
	{
		QSBPlayerManager.LocalPlayer.SuitedUp = Data;
		var animator = QSBPlayerManager.LocalPlayer.AnimationSync;
		animator.InSuitedUpState = Data;
	}
}