using QSB.Animation.NPC.WorldObjects;
using QSB.Events;
using QSB.PoolSync;
using QSB.ShipSync;
using QSB.Utility;
using QSB.WorldSync;
using QSB.WorldSync.Events;

namespace QSB.Player.Events
{
	internal class EnterLeaveEvent : QSBEvent<EnumWorldObjectMessage<EnterLeaveType>>
	{
		// TODO : sync the things that dont need the worldobjects some other way
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener()
		{
			GlobalMessenger.AddListener(EventNames.PlayerEnterQuantumMoon, () => Handler(EnterLeaveType.EnterMoon));
			GlobalMessenger.AddListener(EventNames.PlayerExitQuantumMoon, () => Handler(EnterLeaveType.ExitMoon));
			GlobalMessenger.AddListener(EventNames.QSBEnterShrine, () => Handler(EnterLeaveType.EnterShrine));
			GlobalMessenger.AddListener(EventNames.QSBExitShrine, () => Handler(EnterLeaveType.ExitShrine));
			GlobalMessenger<int>.AddListener(EventNames.QSBEnterPlatform, (int id) => Handler(EnterLeaveType.EnterPlatform, id));
			GlobalMessenger<int>.AddListener(EventNames.QSBExitPlatform, (int id) => Handler(EnterLeaveType.ExitPlatform, id));
			GlobalMessenger<int>.AddListener(EventNames.QSBEnterNonNomaiHeadZone, (int id) => Handler(EnterLeaveType.EnterNonNomaiHeadZone, id));
			GlobalMessenger<int>.AddListener(EventNames.QSBExitNonNomaiHeadZone, (int id) => Handler(EnterLeaveType.ExitNonNomaiHeadZone, id));
			GlobalMessenger<int>.AddListener(EventNames.QSBEnterNomaiHeadZone, (int id) => Handler(EnterLeaveType.EnterNomaiHeadZone, id));
			GlobalMessenger<int>.AddListener(EventNames.QSBExitNomaiHeadZone, (int id) => Handler(EnterLeaveType.ExitNomaiHeadZone, id));
			GlobalMessenger.AddListener(EventNames.EnterShip, () => Handler(EnterLeaveType.EnterShip));
			GlobalMessenger.AddListener(EventNames.ExitShip, () => Handler(EnterLeaveType.ExitShip));
		}

		public override void CloseListener()
		{
			GlobalMessenger.RemoveListener(EventNames.PlayerEnterQuantumMoon, () => Handler(EnterLeaveType.EnterMoon));
			GlobalMessenger.RemoveListener(EventNames.PlayerExitQuantumMoon, () => Handler(EnterLeaveType.ExitMoon));
			GlobalMessenger.RemoveListener(EventNames.QSBEnterShrine, () => Handler(EnterLeaveType.EnterShrine));
			GlobalMessenger.RemoveListener(EventNames.QSBExitShrine, () => Handler(EnterLeaveType.ExitShrine));
			GlobalMessenger<int>.RemoveListener(EventNames.QSBEnterPlatform, (int id) => Handler(EnterLeaveType.EnterPlatform, id));
			GlobalMessenger<int>.RemoveListener(EventNames.QSBExitPlatform, (int id) => Handler(EnterLeaveType.ExitPlatform, id));
			GlobalMessenger<int>.RemoveListener(EventNames.QSBEnterNonNomaiHeadZone, (int id) => Handler(EnterLeaveType.EnterNonNomaiHeadZone, id));
			GlobalMessenger<int>.RemoveListener(EventNames.QSBExitNonNomaiHeadZone, (int id) => Handler(EnterLeaveType.ExitNonNomaiHeadZone, id));
			GlobalMessenger.RemoveListener(EventNames.EnterShip, () => Handler(EnterLeaveType.EnterShip));
			GlobalMessenger.RemoveListener(EventNames.ExitShip, () => Handler(EnterLeaveType.ExitShip));
		}

		private void Handler(EnterLeaveType type, int objectId = -1) => SendEvent(CreateMessage(type, objectId));

		private EnumWorldObjectMessage<EnterLeaveType> CreateMessage(EnterLeaveType type, int objectId) => new()
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
				case EnterLeaveType.EnterNonNomaiHeadZone:
					QSBWorldSync.GetWorldFromId<QSBCharacterAnimController>(message.ObjectId).AddPlayerToHeadZone(player);
					break;
				case EnterLeaveType.ExitNonNomaiHeadZone:
					QSBWorldSync.GetWorldFromId<QSBCharacterAnimController>(message.ObjectId).RemovePlayerFromHeadZone(player);
					break;
				case EnterLeaveType.EnterNomaiHeadZone:
					QSBWorldSync.GetWorldFromId<QSBSolanumAnimController>(message.ObjectId).AddPlayerToHeadZone(player);
					break;
				case EnterLeaveType.ExitNomaiHeadZone:
					QSBWorldSync.GetWorldFromId<QSBSolanumAnimController>(message.ObjectId).RemovePlayerFromHeadZone(player);
					break;
				case EnterLeaveType.EnterShip:
					ShipManager.Instance.AddPlayerToShip(player);
					break;
				case EnterLeaveType.ExitShip:
					ShipManager.Instance.RemovePlayerFromShip(player);
					break;
				default:
					DebugLog.ToConsole($"Warning - Unknown EnterLeaveType : {message.EnumValue}", OWML.Common.MessageType.Warning);
					break;
			}
		}
	}
}
