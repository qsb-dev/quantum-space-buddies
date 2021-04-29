using QSB.Animation.NPC.WorldObjects;
using QSB.Events;
using QSB.PoolSync;
using QSB.Utility;
using QSB.WorldSync;
using QSB.WorldSync.Events;

namespace QSB.Player.Events
{
	internal class EnterLeaveEvent : QSBEvent<EnumWorldObjectMessage<EnterLeaveType>>
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
			GlobalMessenger<int>.AddListener(EventNames.QSBEnterHeadZone, (int id) => Handler(EnterLeaveType.EnterHeadZone, id));
			GlobalMessenger<int>.AddListener(EventNames.QSBExitHeadZone, (int id) => Handler(EnterLeaveType.ExitHeadZone, id));
		}

		public override void CloseListener()
		{
			GlobalMessenger.RemoveListener(EventNames.EnterQuantumMoon, () => Handler(EnterLeaveType.EnterMoon));
			GlobalMessenger.RemoveListener(EventNames.ExitQuantumMoon, () => Handler(EnterLeaveType.ExitMoon));
			GlobalMessenger.RemoveListener(EventNames.QSBEnterShrine, () => Handler(EnterLeaveType.EnterShrine));
			GlobalMessenger.RemoveListener(EventNames.QSBExitShrine, () => Handler(EnterLeaveType.ExitShrine));
		}

		private void Handler(EnterLeaveType type, int objectId = -1) => SendEvent(CreateMessage(type, objectId));

		private EnumWorldObjectMessage<EnterLeaveType> CreateMessage(EnterLeaveType type, int objectId) => new EnumWorldObjectMessage<EnterLeaveType>
		{
			AboutId = LocalPlayerId,
			EnumValue = type,
			ObjectId = objectId
		};

		public override void OnReceiveLocal(bool server, EnumWorldObjectMessage<EnterLeaveType> message)
			=> OnReceiveRemote(server, message);

		public override void OnReceiveRemote(bool server, EnumWorldObjectMessage<EnterLeaveType> message)
		{
			var player = QSBPlayerManager.GetPlayer(message.FromId);
			switch (message.EnumValue)
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
				case EnterLeaveType.EnterHeadZone:
					QSBWorldSync.GetWorldFromId<QSBCharacterAnimController>(message.ObjectId).AddPlayerToHeadZone(player);
					break;
				case EnterLeaveType.ExitHeadZone:
					QSBWorldSync.GetWorldFromId<QSBCharacterAnimController>(message.ObjectId).RemovePlayerFromHeadZone(player);
					break;
				default:
					DebugLog.ToConsole($"Warning - Unknown EnterLeaveType : {message.EnumValue}", OWML.Common.MessageType.Warning);
					break;
			}
		}
	}
}
