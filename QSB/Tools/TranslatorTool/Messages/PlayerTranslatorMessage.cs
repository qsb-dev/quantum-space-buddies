using QSB.Messaging;
using QSB.Player;
using QSB.Player.TransformSync;

namespace QSB.Tools.TranslatorTool.Messages
{
	public class PlayerTranslatorMessage : QSBBoolMessage
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

		private PlayerTranslatorMessage(bool equipped) => Value = equipped;

		public PlayerTranslatorMessage() { }

		public override void OnReceiveRemote()
		{
			var player = QSBPlayerManager.GetPlayer(From);
			player.TranslatorEquipped = Value;
			player.Translator?.ChangeEquipState(Value);
		}

		public override void OnReceiveLocal() =>
			QSBPlayerManager.LocalPlayer.TranslatorEquipped = Value;
	}
}