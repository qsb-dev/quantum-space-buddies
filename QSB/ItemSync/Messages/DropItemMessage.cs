using QSB.ItemSync.WorldObjects.Items;
using QSB.Messaging;
using QSB.Player;
using QSB.SectorSync.WorldObjects;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.ItemSync.Messages;

internal class DropItemMessage : QSBWorldObjectMessage<IQSBItem,
	(Vector3 LocalPos, Vector3 LocalNorm, int SectorId)>
{
	public DropItemMessage(Vector3 position, Vector3 normal, Sector sector) : base((
		sector.transform.InverseTransformPoint(position),
		sector.transform.InverseTransformDirection(normal),
		sector.GetWorldObject<QSBSector>().ObjectId
	)) { }

	public override void OnReceiveRemote()
	{
		var sector = Data.SectorId.GetWorldObject<QSBSector>().AttachedObject;
		var position = sector.transform.TransformPoint(Data.LocalPos);
		var normal = sector.transform.TransformDirection(Data.LocalNorm);
		WorldObject.DropItem(position, normal, sector);

		var player = QSBPlayerManager.GetPlayer(From);
		player.HeldItem = WorldObject;

		player.AnimationSync.VisibleAnimator.SetTrigger("DropHeldItem");
	}
}
