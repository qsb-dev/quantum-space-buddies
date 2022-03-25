using Cysharp.Threading.Tasks;
using QSB.ItemSync.WorldObjects.Sockets;
using QSB.Patches;
using QSB.Player;
using QSB.SectorSync.WorldObjects;
using QSB.WorldSync;
using System.Threading;
using UnityEngine;

namespace QSB.ItemSync.WorldObjects.Items;

public class QSBItem<T> : WorldObject<T>, IQSBItem
	where T : OWItem
{
	private Transform _lastParent;
	private Vector3 _lastPosition;
	private Quaternion _lastRotation;
	private QSBSector _lastSector;
	private QSBItemSocket _lastSocket;

	public override async UniTask Init(CancellationToken ct)
	{
		await UniTask.WaitUntil(() => QSBWorldSync.AllObjectsAdded, cancellationToken: ct);

		StoreLocation();

		QSBPlayerManager.OnRemovePlayer += OnPlayerLeave;
	}

	public override void OnRemoval() => QSBPlayerManager.OnRemovePlayer -= OnPlayerLeave;

	public void StoreLocation()
	{
		_lastParent = AttachedObject.transform.parent;
		_lastPosition = AttachedObject.transform.localPosition;
		_lastRotation = AttachedObject.transform.localRotation;

		var sector = AttachedObject.GetSector();
		if (sector != null)
		{
			_lastSector = sector.GetWorldObject<QSBSector>();
		}

		var socket = _lastParent.GetComponent<OWItemSocket>();
		if (socket != null)
		{
			_lastSocket = socket.GetWorldObject<QSBItemSocket>();
		}
	}

	private void OnPlayerLeave(PlayerInfo player)
	{
		if (player.HeldItem != this)
		{
			return;
		}

		if (_lastSocket != null)
		{
			QSBPatch.RemoteCall(() => _lastSocket.PlaceIntoSocket(this));
		}
		else
		{
			AttachedObject.transform.parent = _lastParent;
			AttachedObject.transform.localPosition = _lastPosition;
			AttachedObject.transform.localRotation = _lastRotation;
			AttachedObject.transform.localScale = Vector3.one;
			AttachedObject.SetSector(_lastSector?.AttachedObject);
			AttachedObject.SetColliderActivation(true);
		}
	}

	public override void SendInitialState(uint to)
	{
		// todo SendInitialState
	}

	public ItemType GetItemType() => AttachedObject.GetItemType();

	public void PickUpItem(Transform holdTransform) =>
		QSBPatch.RemoteCall(() => AttachedObject.PickUpItem(holdTransform));

	public void DropItem(Vector3 position, Vector3 normal, Sector sector) =>
		QSBPatch.RemoteCall(() => AttachedObject.DropItem(position, normal, sector.transform, sector, null));

	public void OnCompleteUnsocket() => AttachedObject.OnCompleteUnsocket();
}
