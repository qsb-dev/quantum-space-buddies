using Cysharp.Threading.Tasks;
using QSB.ItemSync.WorldObjects.Sockets;
using QSB.Player;
using QSB.SectorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Threading;
using UnityEngine;

namespace QSB.ItemSync.WorldObjects.Items;

internal class QSBItem<T> : WorldObject<T>, IQSBItem
	where T : OWItem
{
	private QSBItemSocket InitialSocket { get; set; }
	private Transform InitialParent { get; set; }
	private Vector3 InitialPosition { get; set; }
	private Quaternion InitialRotation { get; set; }
	private QSBSector InitialSector { get; set; }

	public override async UniTask Init(CancellationToken ct)
	{
		if (AttachedObject == null)
		{
			DebugLog.ToConsole($"Error - AttachedObject is null! Type:{GetType().Name}", OWML.Common.MessageType.Error);
			return;
		}

		await UniTask.WaitUntil(() => QSBWorldSync.AllObjectsAdded, cancellationToken: ct);

		InitialParent = AttachedObject.transform.parent;
		InitialPosition = AttachedObject.transform.localPosition;
		InitialRotation = AttachedObject.transform.localRotation;
		var initialSector = AttachedObject.GetSector();
		if (initialSector != null)
		{
			InitialSector = initialSector.GetWorldObject<QSBSector>();
		}

		if (InitialParent == null)
		{
			DebugLog.ToConsole($"Warning - InitialParent of {AttachedObject.name} is null!", OWML.Common.MessageType.Warning);
		}

		if (InitialParent?.GetComponent<OWItemSocket>() != null)
		{
			var qsbObj = InitialParent.GetComponent<OWItemSocket>().GetWorldObject<QSBItemSocket>();
			InitialSocket = qsbObj;
		}

		QSBPlayerManager.OnRemovePlayer += OnPlayerLeave;
	}

	public override void OnRemoval() => QSBPlayerManager.OnRemovePlayer -= OnPlayerLeave;

	private void OnPlayerLeave(PlayerInfo player)
	{
		if (player.HeldItem != this)
		{
			return;
		}

		if (InitialSocket != null)
		{
			InitialSocket.PlaceIntoSocket(this);
			return;
		}

		AttachedObject.transform.parent = InitialParent;
		AttachedObject.transform.localPosition = InitialPosition;
		AttachedObject.transform.localRotation = InitialRotation;
		AttachedObject.transform.localScale = Vector3.one;
		AttachedObject.SetSector(InitialSector?.AttachedObject);
		AttachedObject.SetColliderActivation(true);
	}

	public override void SendInitialState(uint to)
	{
		// todo SendInitialState
	}

	public ItemType GetItemType()
		=> AttachedObject.GetItemType();

	public void PickUpItem(Transform holdTransform)
		=> AttachedObject.PickUpItem(holdTransform);

	public void DropItem(Vector3 position, Vector3 normal, Sector sector) =>
		AttachedObject.DropItem(sector.transform.TransformPoint(position), normal, sector.transform, sector, null);

	public void OnCompleteUnsocket()
		=> AttachedObject.OnCompleteUnsocket();
}