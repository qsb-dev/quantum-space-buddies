using QSB.Messaging;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.ShipSync;

namespace QSB.Tools.SignalscopeTool.Messages;

public class PlayerSignalscopeMessage : QSBMessage<bool>
{
	static PlayerSignalscopeMessage()
	{
		GlobalMessenger<Signalscope>.AddListener(OWEvents.EquipSignalscope, _ => Handle(true));
		GlobalMessenger.AddListener(OWEvents.UnequipSignalscope, () => Handle(false));
	}

	private static void Handle(bool equipped)
	{
		if (PlayerTransformSync.LocalInstance)
		{
			new PlayerSignalscopeMessage(equipped).Send();
		}
	}

	private PlayerSignalscopeMessage(bool equipped) : base(equipped) { }

	public override void OnReceiveRemote()
	{
		var player = QSBPlayerManager.GetPlayer(From);
		player.SignalscopeEquipped = Data;
		player.Signalscope?.ChangeEquipState(Data);

		if (player.FlyingShip)
		{
			ShipManager.Instance.UpdateSignalscope(Data);
		}
	}

	public override void OnReceiveLocal() =>
		QSBPlayerManager.LocalPlayer.SignalscopeEquipped = Data;
}