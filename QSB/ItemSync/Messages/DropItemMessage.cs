using Mirror;
using QSB.ItemSync.WorldObjects.Items;
using QSB.Messaging;
using QSB.Player;
using QSB.SectorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.ItemSync.Messages;

internal class DropItemMessage : QSBWorldObjectMessage<IQSBItem>
{
	private Vector3 Position;
	private Vector3 Normal;
	private int SectorId;

	public DropItemMessage(Vector3 position, Vector3 normal, Sector sector)
	{
		Position = position;
		Normal = normal;
		SectorId = sector.GetWorldObject<QSBSector>().ObjectId;
	}

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.Write(Position);
		writer.Write(Normal);
		writer.Write(SectorId);
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		Position = reader.ReadVector3();
		Normal = reader.ReadVector3();
		SectorId = reader.Read<int>();
	}

	public override void OnReceiveRemote()
	{
		var sector = SectorId.GetWorldObject<QSBSector>().AttachedObject;
		WorldObject.DropItem(Position, Normal, sector);

		var player = QSBPlayerManager.GetPlayer(From);
		player.HeldItem = WorldObject;

		DebugLog.DebugWrite("DROP HELD ITEM");
		player.AnimationSync.VisibleAnimator.SetTrigger("DropHeldItem");
	}
}