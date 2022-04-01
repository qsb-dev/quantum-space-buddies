using QSB.ItemSync.WorldObjects;
using QSB.ItemSync.WorldObjects.Items;
using QSB.Messaging;
using QSB.Player;
using QSB.SectorSync.WorldObjects;
using QSB.WorldSync;
using QSB.WorldSync.WorldObjects;
using UnityEngine;

namespace QSB.ItemSync.Messages;

internal class DropItemMessage : QSBWorldObjectMessage<IQSBItem, (Vector3 localPosition, Vector3 localNormal, int sectorId, int dropTargetId, int rigidBodyId)>
{
	public DropItemMessage(Vector3 worldPosition, Vector3 worldNormal, Transform parent, Sector sector, MonoBehaviourWorldObject customDropTarget, OWRigidbody targetRigidbody)
		: base(ProcessInputs(worldPosition, worldNormal, parent, sector, customDropTarget, targetRigidbody)) { }

	private static (Vector3 localPosition, Vector3 localNormal, int sectorId, int dropTargetId, int rigidBodyId) ProcessInputs(
		Vector3 worldPosition,
		Vector3 worldNormal,
		Transform parent,
		Sector sector,
		MonoBehaviourWorldObject customDropTarget,
		OWRigidbody targetRigidbody)
	{
		(Vector3 localPosition, Vector3 localNormal, int sectorId, int dropTargetId, int rigidBodyId) tuple = new();

		if (customDropTarget == null)
		{
			tuple.rigidBodyId = targetRigidbody.GetWorldObject<QSBOWRigidbody>().ObjectId;
			tuple.dropTargetId = -1;
		}
		else
		{
			tuple.rigidBodyId = -1;
			tuple.dropTargetId = customDropTarget.ObjectId;
		}

		tuple.sectorId = sector.GetWorldObject<QSBSector>().ObjectId;
		tuple.localPosition = parent.InverseTransformPoint(worldPosition);
		tuple.localNormal = parent.InverseTransformDirection(worldNormal);

		return tuple;
	}

	public override void OnReceiveRemote()
	{
		var customDropTarget = (Data.dropTargetId == -1)
			? null
			: QSBWorldSync.GetWorldObject<MonoBehaviourWorldObject>(Data.dropTargetId).AttachedObject as IItemDropTarget;

		var parent = (Data.dropTargetId == -1)
			? QSBWorldSync.GetWorldObject<QSBOWRigidbody>(Data.rigidBodyId).AttachedObject.transform
			: customDropTarget.GetItemDropTargetTransform(null);

		var worldPos = parent.TransformPoint(Data.localPosition);
		var worldNormal = parent.TransformDirection(Data.localNormal);

		var sector = QSBWorldSync.GetWorldObject<QSBSector>(Data.sectorId).AttachedObject;

		WorldObject.DropItem(worldPos, worldNormal, parent, sector, customDropTarget);

		var player = QSBPlayerManager.GetPlayer(From);
		player.HeldItem = null;
		player.AnimationSync.VisibleAnimator.SetTrigger("DropHeldItem");
	}
}
