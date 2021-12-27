using OWML.Common;
using QSB.Animation.NPC.WorldObjects;
using QSB.Messaging;
using QSB.Player.TransformSync;
using QSB.PoolSync;
using QSB.ShipSync;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET.Transport;

namespace QSB.Player.Messages
{
	internal class EnterLeaveMessage : QSBEnumMessage<EnterLeaveType>
	{
		static EnterLeaveMessage()
		{
			GlobalMessenger.AddListener(OWEvents.PlayerEnterQuantumMoon, () => Handler(EnterLeaveType.EnterMoon));
			GlobalMessenger.AddListener(OWEvents.PlayerExitQuantumMoon, () => Handler(EnterLeaveType.ExitMoon));
			GlobalMessenger.AddListener(OWEvents.EnterShip, () => Handler(EnterLeaveType.EnterShip));
			GlobalMessenger.AddListener(OWEvents.ExitShip, () => Handler(EnterLeaveType.ExitShip));
		}

		private static void Handler(EnterLeaveType type, int objectId = -1)
		{
			if (PlayerTransformSync.LocalInstance)
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
					ObjectId.GetWorldObject<QSBCharacterAnimController>().AddPlayerToHeadZone(player);
					break;
				case EnterLeaveType.ExitNonNomaiHeadZone:
					ObjectId.GetWorldObject<QSBCharacterAnimController>().RemovePlayerFromHeadZone(player);
					break;
				case EnterLeaveType.EnterNomaiHeadZone:
					ObjectId.GetWorldObject<QSBSolanumAnimController>().AddPlayerToHeadZone(player);
					break;
				case EnterLeaveType.ExitNomaiHeadZone:
					ObjectId.GetWorldObject<QSBSolanumAnimController>().RemovePlayerFromHeadZone(player);
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