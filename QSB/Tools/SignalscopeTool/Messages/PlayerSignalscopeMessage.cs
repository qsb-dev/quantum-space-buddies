using QSB.Messaging;
using QSB.Player;
using QSB.Player.TransformSync;

namespace QSB.Tools.SignalscopeTool.Messages
{
	public class PlayerSignalscopeMessage : QSBBoolMessage
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

		private PlayerSignalscopeMessage(bool equipped) => Value = equipped;

		public PlayerSignalscopeMessage() { }

		public override void OnReceiveRemote()
		{
			var player = QSBPlayerManager.GetPlayer(From);
			player.SignalscopeEquipped = Value;
			player.Signalscope?.ChangeEquipState(Value);
		}

		public override void OnReceiveLocal() =>
			QSBPlayerManager.LocalPlayer.SignalscopeEquipped = Value;
	}
}