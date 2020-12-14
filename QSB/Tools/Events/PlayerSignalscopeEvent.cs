using QSB.Events;
using QSB.Messages;
using QSB.Player;

namespace QSB.Tools.Events
{
	public class PlayerSignalscopeEvent : QSBEvent<ToggleMessage>
	{
		public override EventType Type => EventType.SignalscopeActiveChange;

		public override void SetupListener()
		{
			GlobalMessenger<Signalscope>.AddListener(EventNames.EquipSignalscope, HandleEquip);
			GlobalMessenger.AddListener(EventNames.UnequipSignalscope, HandleUnequip);
		}

		public override void CloseListener()
		{
			GlobalMessenger<Signalscope>.RemoveListener(EventNames.EquipSignalscope, HandleEquip);
			GlobalMessenger.RemoveListener(EventNames.UnequipSignalscope, HandleUnequip);
		}

		private void HandleEquip(Signalscope var) => SendEvent(CreateMessage(true));
		private void HandleUnequip() => SendEvent(CreateMessage(false));

		private ToggleMessage CreateMessage(bool value) => new ToggleMessage
		{
			AboutId = LocalPlayerId,
			ToggleValue = value
		};

		public override void OnReceiveRemote(bool server, ToggleMessage message)
		{
			var player = QSBPlayerManager.GetPlayer(message.AboutId);
			player.UpdateState(State.Signalscope, message.ToggleValue);
			player.Signalscope?.ChangeEquipState(message.ToggleValue);
		}

		public override void OnReceiveLocal(bool server, ToggleMessage message)
		{
			QSBPlayerManager.LocalPlayer.UpdateState(State.Signalscope, message.ToggleValue);
		}
	}
}