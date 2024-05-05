﻿using Cysharp.Threading.Tasks;
using OWML.Common;
using QSB.ItemSync.Messages;
using QSB.ItemSync.WorldObjects.Sockets;
using QSB.Messaging;
using QSB.Player;
using QSB.SectorSync.WorldObjects;
using QSB.Utility;
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
			_lastSocket.PlaceIntoSocket(this);
		}
		else
		{
			// TODO at some point we should probably call the proper drop item code to account for funny overrides
			AttachedObject.transform.parent = _lastParent;
			AttachedObject.transform.localPosition = _lastPosition;
			AttachedObject.transform.localRotation = _lastRotation;
			AttachedObject.transform.localScale = Vector3.one;
			AttachedObject.SetSector(_lastSector?.AttachedObject);
			AttachedObject.SetColliderActivation(true);
		}
	}

	public ItemType GetItemType() => AttachedObject.GetItemType();

	public virtual void PickUpItem(Transform holdTransform) =>
		AttachedObject.PickUpItem(holdTransform);

	public virtual void DropItem(Vector3 worldPosition, Vector3 worldNormal, Transform parent, Sector sector, IItemDropTarget customDropTarget) =>
		AttachedObject.DropItem(worldPosition, worldNormal, parent, sector, customDropTarget);

	public void OnCompleteUnsocket() => AttachedObject.OnCompleteUnsocket();
}
