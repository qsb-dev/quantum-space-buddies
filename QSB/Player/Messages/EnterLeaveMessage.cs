using OWML.Common;
using QSB.Animation.NPC.WorldObjects;
using QSB.Events;
using QSB.Messaging;
using QSB.Player.TransformSync;
using QSB.PoolSync;
using QSB.ShipSync;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET.Transport;

namespace QSB.Player.Messages
{
	// TODO: maybe one day split this up into multiple messages
	internal class EnterLeaveMessage : QSBEnumMessage<EnterLeaveType>
	{
		static EnterLeaveMessage()
		{
			GlobalMessenger.AddListener(EventNames.PlayerEnterQuantumMoon, () => Handler(EnterLeaveType.EnterMoon));
			GlobalMessenger.AddListener(EventNames.PlayerExitQuantumMoon, () => Handler(EnterLeaveType.ExitMoon));
			GlobalMessenger.AddListener(EventNames.EnterShip, () => Handler(EnterLeaveType.EnterShip));
			GlobalMessenger.AddListener(EventNames.ExitShip, () => Handler(EnterLeaveType.ExitShip));
		}

		private static void Handler(EnterLeaveType type, int objectId = -1)
		{
			if (PlayerTransformSync.LocalInstance != null)
			{
				new EnterLeaveMessage(type, objectId).Send();
			}
		}


		private int ObjectId;

		public EnterLeaveMessage(EnterLeaveType type, int objectId = -1)
		{
			Value = type;
			ObjectId = objectId;
		}

		public EnterLeaveMessage() { }

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(ObjectId);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			ObjectId = reader.ReadInt32();
		}

		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;

		public override void OnReceiveLocal() => OnReceiveRemote();

		public override void OnReceiveRemote()
		{
			var player = QSBPlayerManager.GetPlayer(From);
			switch (Value)
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
					CustomNomaiRemoteCameraPlatform.CustomPlatformList[ObjectId]
						.OnRemotePlayerEnter(From);
					break;
				case EnterLeaveType.ExitPlatform:
					CustomNomaiRemoteCameraPlatform.CustomPlatformList[ObjectId]
						.OnRemotePlayerExit(From);
					break;
				case EnterLeaveType.EnterNonNomaiHeadZone:
					QSBWorldSync.GetWorldFromId<QSBCharacterAnimController>(ObjectId).AddPlayerToHeadZone(player);
					break;
				case EnterLeaveType.ExitNonNomaiHeadZone:
					QSBWorldSync.GetWorldFromId<QSBCharacterAnimController>(ObjectId).RemovePlayerFromHeadZone(player);
					break;
				case EnterLeaveType.EnterNomaiHeadZone:
					QSBWorldSync.GetWorldFromId<QSBSolanumAnimController>(ObjectId).AddPlayerToHeadZone(player);
					break;
				case EnterLeaveType.ExitNomaiHeadZone:
					QSBWorldSync.GetWorldFromId<QSBSolanumAnimController>(ObjectId).RemovePlayerFromHeadZone(player);
					break;
				case EnterLeaveType.EnterShip:
					ShipManager.Instance.AddPlayerToShip(player);
					break;
				case EnterLeaveType.ExitShip:
					ShipManager.Instance.RemovePlayerFromShip(player);
					break;
				default:
					DebugLog.ToConsole($"Warning - Unknown EnterLeaveType : {Value}", MessageType.Warning);
					break;
			}
		}
	}
}