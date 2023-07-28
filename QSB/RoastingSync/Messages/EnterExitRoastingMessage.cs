using Mirror;
using OWML.Common;
using QSB.CampfireSync.WorldObjects;
using QSB.Messaging;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.RoastingSync.Messages;

public class EnterExitRoastingMessage : QSBMessage<bool>
{
	static EnterExitRoastingMessage()
	{
		GlobalMessenger<Campfire>.AddListener(OWEvents.EnterRoastingMode, campfire => Handler(campfire, true));
		GlobalMessenger.AddListener(OWEvents.ExitRoastingMode, () => Handler(null, false));
	}

	private static void Handler(Campfire campfire, bool roasting)
	{
		if (PlayerTransformSync.LocalInstance)
		{
			if (campfire == null)
			{
				new EnterExitRoastingMessage(-1, roasting).Send();
				return;
			}

			var qsbObj = campfire.GetWorldObject<QSBCampfire>();
			new EnterExitRoastingMessage(qsbObj.ObjectId, roasting).Send();
		}
	}

	private int ObjectId;

	private EnterExitRoastingMessage(int objectId, bool roasting) : base(roasting) =>
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

	public override void OnReceiveRemote()
	{
		if (Data && ObjectId == -1)
		{
			DebugLog.ToConsole($"Error - Null campfire supplied for start roasting event!", MessageType.Error);
			return;
		}

		var player = QSBPlayerManager.GetPlayer(From);
		player.RoastingStick.SetActive(Data);
		player.Campfire = Data
			? ObjectId.GetWorldObject<QSBCampfire>()
			: null;
	}
}