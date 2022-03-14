using Cysharp.Threading.Tasks;
using Mirror;
using QSB.Messaging;
using QSB.OrbSync.Messages;
using QSB.OrbSync.TransformSync;
using QSB.Utility;
using QSB.Utility.LinkedWorldObject;
using QSB.WorldSync;
using System;
using System.Threading;
using UnityEngine;

namespace QSB.OrbSync.WorldObjects;

public class QSBOrb : WorldObject<NomaiInterfaceOrb>, ILinkedWorldObject<NomaiOrbTransformSync>
{
	public override bool ShouldDisplayDebug() => false;

	public NomaiOrbTransformSync NetworkBehaviour { get; private set; }
	public void LinkTo(NetworkBehaviour networkBehaviour) => NetworkBehaviour = (NomaiOrbTransformSync)networkBehaviour;

	public override async UniTask Init(CancellationToken ct)
	{
		if (QSBCore.IsHost)
		{
			this.SpawnLinked(QSBNetworkManager.singleton.OrbPrefab);
		}
		else
		{
			await this.WaitForLink(ct);
		}
	}

	public override void OnRemoval()
	{
		if (QSBCore.IsHost)
		{
			NetworkServer.Destroy(NetworkBehaviour.gameObject);
		}
	}

	public override void SendInitialState(uint to)
	{
		this.SendMessage(new OrbDragMessage(AttachedObject._isBeingDragged) { To = to });
		var slotIndex = Array.IndexOf(AttachedObject._slots, AttachedObject._occupiedSlot);
		this.SendMessage(new OrbSlotMessage(slotIndex, false) { To = to });
	}

	public void SetDragging(bool value)
	{
		if (value == AttachedObject._isBeingDragged)
		{
			return;
		}

		if (value)
		{
			AttachedObject._isBeingDragged = true;
			AttachedObject._interactibleCollider.enabled = false;
			if (AttachedObject._orbAudio != null)
			{
				AttachedObject._orbAudio.PlayStartDragClip();
			}
		}
		else
		{
			AttachedObject._isBeingDragged = false;
			AttachedObject._interactibleCollider.enabled = true;
		}
	}

	public void SetSlot(int slotIndex, bool playAudio)
	{
		var oldSlot = AttachedObject._occupiedSlot;
		var newSlot = slotIndex == -1 ? null : AttachedObject._slots[slotIndex];
		if (newSlot == oldSlot)
		{
			return;
		}

		if (oldSlot)
		{
			oldSlot._occupyingOrb = null;
			oldSlot.RaiseEvent(nameof(oldSlot.OnSlotDeactivated), oldSlot);

			AttachedObject._occupiedSlot = null;
		}

		if (newSlot)
		{
			newSlot._occupyingOrb = AttachedObject;
			if (Time.timeSinceLevelLoad > 1f)
			{
				newSlot.RaiseEvent(nameof(newSlot.OnSlotActivated), newSlot);
			}

			AttachedObject._occupiedSlot = newSlot;
			AttachedObject._enterSlotTime = Time.time;
			if (newSlot.CancelsDragOnCollision())
			{
				AttachedObject.CancelDrag();
			}

			if (playAudio && AttachedObject._orbAudio != null && newSlot.GetPlayActivationAudio())
			{
				AttachedObject._orbAudio.PlaySlotActivatedClip();
			}
		}
	}
}