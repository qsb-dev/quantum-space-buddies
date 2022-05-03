using QSB.Messaging;
using QSB.Player;
using QSB.Player.TransformSync;

namespace QSB.Tools.TranslatorTool.Messages;

public class PlayerTranslatorMessage : QSBMessage<bool>
{
	static PlayerTranslatorMessage()
	{
		GlobalMessenger.AddListener(OWEvents.EquipTranslator, () => Handle(true));
		GlobalMessenger.AddListener(OWEvents.UnequipTranslator, () => Handle(false));
	}

	private static void Handle(bool equipped)
	{
		if (PlayerTransformSync.LocalInstance)
		{
			new PlayerTranslatorMessage(equipped).Send();
		}
	}

	private PlayerTranslatorMessage(bool equipped) : base(equipped) { }

	public override void OnReceiveRemote()
	{
		var player = QSBPlayerManager.GetPlayer(From);
		player.TranslatorEquipped = Data;
		player.Translator?.ChangeEquipState(Data);
	}

	public override void OnReceiveLocal() =>
		QSBPlayerManager.LocalPlayer.TranslatorEquipped = Data;
}