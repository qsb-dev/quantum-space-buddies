using Mirror;
using OWML.Common;
using QSB.Messaging;
using QSB.Player.TransformSync;
using QSB.PoolSync;
using QSB.ShipSync;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.Player.Messages;

/// <summary>
/// todo SendInitialState
/// </summary>
public class EnterLeaveMessage : QSBMessage<EnterLeaveType>
{
	static EnterLeaveMessage()
	{
		GlobalMessenger.AddListener(OWEvents.PlayerEnterQuantumMoon, () => Handler(EnterLeaveType.EnterMoon));
		GlobalMessenger.AddListener(OWEvents.PlayerExitQuantumMoon, () => Handler(EnterLeaveType.ExitMoon));
		GlobalMessenger.AddListener(OWEvents.EnterShip, () => Handler(EnterLeaveType.EnterShip));
		GlobalMessenger.AddListener(OWEvents.ExitShip, () => Handler(EnterLeaveType.ExitShip));
		GlobalMessenger.AddListener(OWEvents.EnterCloak, () => Handler(EnterLeaveType.EnterCloak));
		GlobalMessenger.AddListener(OWEvents.ExitCloak, () => Handler(EnterLeaveType.ExitCloak));
	}

	private static void Handler(EnterLeaveType type, int objectId = -1)
	{
		if (PlayerTransformSync.LocalInstance)
		{
			new EnterLeaveMessage(type, objectId).Send();
		}
	}

	private int ObjectId;

	public EnterLeaveMessage(EnterLeaveType type, int objectId = -1) : base(type) =>
		ObjectId = objectId;

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.Write(ObjectId);
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		ObjectId = reader.Read<int>();
	}

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveLocal() => OnReceiveRemote();

	public override void OnReceiveRemote()
	{
		var player = QSBPlayerManager.GetPlayer(From);
		switch (Data)
		{
			case EnterLeaveType.EnterMoon:
				player.IsInMoon = true;
				player.HUDBox.OnEnterUnknown();
				break;
			case EnterLeaveType.ExitMoon:
				player.IsInMoon = false;
				player.HUDBox.OnExitUnknown();
				break;
			case EnterLeaveType.EnterCloak:
				player.IsInCloak = true;
				player.HUDBox.OnEnterUnknown();
				break;
			case EnterLeaveType.ExitCloak:
				player.IsInCloak = false;
				player.HUDBox.OnExitUnknown();
				break;
			case EnterLeaveType.EnterPlatform:
				CustomNomaiRemoteCameraPlatform.CustomPlatformList[ObjectId]
					.OnRemotePlayerEnter(From);
				break;
			case EnterLeaveType.ExitPlatform:
				CustomNomaiRemoteCameraPlatform.CustomPlatformList[ObjectId]
					.OnRemotePlayerExit(From);
				break;
			case EnterLeaveType.EnterShip:
				ShipManager.Instance.AddPlayerToShip(player);
				break;
			case EnterLeaveType.ExitShip:
				ShipManager.Instance.RemovePlayerFromShip(player);
				break;
			default:
				DebugLog.ToConsole($"Warning - Unknown EnterLeaveType : {Data}", MessageType.Warning);
				break;
		}
	}
}