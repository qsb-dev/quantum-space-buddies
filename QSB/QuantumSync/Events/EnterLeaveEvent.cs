using QSB.Events;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;

namespace QSB.QuantumSync.Events
{
	internal class EnterLeaveEvent : QSBEvent<EnumMessage<EnterLeaveType>>
	{
		public override EventType Type => EventType.EnterLeave;

		public override void SetupListener()
		{
			GlobalMessenger.AddListener(EventNames.EnterQuantumMoon, () => Handler(EnterLeaveType.EnterMoon));
			GlobalMessenger.AddListener(EventNames.ExitQuantumMoon, () => Handler(EnterLeaveType.ExitMoon));
			GlobalMessenger.AddListener(EventNames.QSBEnterShrine, () => Handler(EnterLeaveType.EnterShrine));
			GlobalMessenger.AddListener(EventNames.QSBExitShrine, () => Handler(EnterLeaveType.ExitShrine));
		}

		public override void CloseListener()
		{
			GlobalMessenger.RemoveListener(EventNames.EnterQuantumMoon, () => Handler(EnterLeaveType.EnterMoon));
			GlobalMessenger.RemoveListener(EventNames.ExitQuantumMoon, () => Handler(EnterLeaveType.ExitMoon));
			GlobalMessenger.RemoveListener(EventNames.QSBEnterShrine, () => Handler(EnterLeaveType.EnterShrine));
			GlobalMessenger.RemoveListener(EventNames.QSBExitShrine, () => Handler(EnterLeaveType.ExitShrine));
		}

		private void Handler(EnterLeaveType type) => SendEvent(CreateMessage(type));

		private EnumMessage<EnterLeaveType> CreateMessage(EnterLeaveType type) => new EnumMessage<EnterLeaveType>
		{
			AboutId = LocalPlayerId,
			Value = type
		};

		public override void OnReceiveLocal(bool server, EnumMessage<EnterLeaveType> message)
			=> OnReceiveRemote(server, message);

		public override void OnReceiveRemote(bool server, EnumMessage<EnterLeaveType> message)
		{
			var player = QSBPlayerManager.GetPlayer(message.FromId);
			switch (message.Value)
			{
				case EnterLeaveType.EnterMoon:
					player.IsInMoon = true;
					break;
				case EnterLeaveType.ExitMoon:
					player.IsInMoon = false;
					break;
				case EnterLeaveType.EnterShrine:
					player.IsInShrine = true;
					break;
				case EnterLeaveType.ExitShrine:
					player.IsInShrine = false;
					break;
				default:
					DebugLog.ToConsole($"Warning - Unknown EnterLeaveType : {message.Value}", OWML.Common.MessageType.Warning);
					break;
			}
		}
	}
}
