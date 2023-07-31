using QSB.ItemSync.WorldObjects;
using QSB.ItemSync.WorldObjects.Items;
using QSB.Messaging;
using QSB.Player;
using QSB.SectorSync.WorldObjects;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.ItemSync.Messages;

public class DropItemMessage : QSBWorldObjectMessage<IQSBItem,
	(Vector3 localPosition, Vector3 localNormal, int sectorId, int dropTargetId, int rigidBodyId)>
{
	public DropItemMessage(Vector3 worldPosition, Vector3 worldNormal, Transform parent, Sector sector, IItemDropTarget customDropTarget, OWRigidbody targetRigidbody)
		: base(ProcessInputs(worldPosition, worldNormal, parent, sector, customDropTarget, targetRigidbody)) { }

	private static (Vector3 localPosition, Vector3 localNormal, int sectorId, int dropTargetId, int rigidBodyId) ProcessInputs(
		Vector3 worldPosition,
		Vector3 worldNormal,
		Transform parent,
		Sector sector,
		IItemDropTarget customDropTarget,
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
			tuple.dropTargetId = ((MonoBehaviour)customDropTarget).GetWorldObject<IQSBDropTarget>().ObjectId;
		}

		tuple.sectorId = sector ? sector.GetWorldObject<QSBSector>().ObjectId : -1;
		tuple.localPosition = parent.InverseTransformPoint(worldPosition);
		tuple.localNormal = parent.InverseTransformDirection(worldNormal);

		return tuple;
	}

	public override void OnReceiveRemote()
	{
		var customDropTarget = Data.dropTargetId == -1
			? null
			: Data.dropTargetId.GetWorldObject<IQSBDropTarget>().AttachedObject;

		var parent = customDropTarget == null
			? Data.rigidBodyId.GetWorldObject<QSBOWRigidbody>().AttachedObject.transform
			: customDropTarget.GetItemDropTargetTransform(null);

		var worldPos = parent.TransformPoint(Data.localPosition);
		var worldNormal = parent.TransformDirection(Data.localNormal);

		var sector = Data.sectorId != -1 ? Data.sectorId.GetWorldObject<QSBSector>().AttachedObject : null;

		WorldObject.DropItem(worldPos, worldNormal, parent, sector, customDropTarget);
		WorldObject.ItemState.HasBeenInteractedWith = true;
		WorldObject.ItemState.State = ItemStateType.OnGround;
		WorldObject.ItemState.LocalPosition = Data.localPosition;
		WorldObject.ItemState.Parent = parent;
		WorldObject.ItemState.LocalNormal = Data.localNormal;
		WorldObject.ItemState.Sector = sector;
		WorldObject.ItemState.CustomDropTarget = customDropTarget;
		WorldObject.ItemState.Rigidbody = parent.GetComponent<OWRigidbody>();

		var player = QSBPlayerManager.GetPlayer(From);
		player.HeldItem = null;
		player.AnimationSync.VisibleAnimator.SetTrigger("DropHeldItem");
	}
}
