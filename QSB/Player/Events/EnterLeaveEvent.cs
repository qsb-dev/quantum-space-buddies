using QSB.Events;
using QSB.ItemSync;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;

namespace QSB.Player.Events
{
	internal class EnterLeaveEvent : QSBEvent<EnterLeaveMessage>
	{
		public override EventType Type => EventType.EnterLeave;

		public override void SetupListener()
		{
			GlobalMessenger.AddListener(EventNames.EnterQuantumMoon, () => Handler(EnterLeaveType.EnterMoon));
			GlobalMessenger.AddListener(EventNames.ExitQuantumMoon, () => Handler(EnterLeaveType.ExitMoon));
			GlobalMessenger.AddListener(EventNames.QSBEnterShrine, () => Handler(EnterLeaveType.EnterShrine));
			GlobalMessenger.AddListener(EventNames.QSBExitShrine, () => Handler(EnterLeaveType.ExitShrine));
			GlobalMessenger<int>.AddListener(EventNames.QSBEnterPlatform, (int id) => Handler(EnterLeaveType.EnterPlatform, id));
			GlobalMessenger<int>.AddListener(EventNames.QSBExitPlatform, (int id) => Handler(EnterLeaveType.ExitPlatform, id));
		}

		public override void CloseListener()
		{
			GlobalMessenger.RemoveListener(EventNames.EnterQuantumMoon, () => Handler(EnterLeaveType.EnterMoon));
			GlobalMessenger.RemoveListener(EventNames.ExitQuantumMoon, () => Handler(EnterLeaveType.ExitMoon));
			GlobalMessenger.RemoveListener(EventNames.QSBEnterShrine, () => Handler(EnterLeaveType.EnterShrine));
			GlobalMessenger.RemoveListener(EventNames.QSBExitShrine, () => Handler(EnterLeaveType.ExitShrine));
		}

		private void Handler(EnterLeaveType type, int objectId = -1) => SendEvent(CreateMessage(type, objectId));

		private EnterLeaveMessage CreateMessage(EnterLeaveType type, int objectId) => new EnterLeaveMessage
		{
			AboutId = LocalPlayerId,
			Type = type,
			ObjectId = objectId
		};

		public override void OnReceiveLocal(bool server, EnterLeaveMessage message)
			=> OnReceiveRemote(server, message);

		public override void OnReceiveRemote(bool server, EnterLeaveMessage message)
		{
			var player = QSBPlayerManager.GetPlayer(message.FromId);
			switch (message.Type)
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
				case EnterLeaveType.EnterPlatform:
					CustomNomaiRemoteCameraPlatform.CustomPlatformList[message.ObjectId]
						.OnRemotePlayerEnter(message.AboutId);
					break;
				case EnterLeaveType.ExitPlatform:
					CustomNomaiRemoteCameraPlatform.CustomPlatformList[message.ObjectId]
						.OnRemotePlayerExit(message.AboutId);
					break;
				default:
					DebugLog.ToConsole($"Warning - Unknown EnterLeaveType : {message.Type}", OWML.Common.MessageType.Warning);
					break;
			}
		}
	}
}
