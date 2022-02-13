using Cysharp.Threading.Tasks;
using Mirror;
using QSB.Messaging;
using QSB.OrbSync.Messages;
using QSB.OrbSync.TransformSync;
using QSB.Utility;
using QSB.WorldSync;
using System.Threading;
using UnityEngine;

namespace QSB.OrbSync.WorldObjects
{
	public class QSBOrb : WorldObject<NomaiInterfaceOrb>
	{
		public override bool ShouldDisplayDebug() => false;

		public NomaiOrbTransformSync TransformSync;

		public override async UniTask Init(CancellationToken ct)
		{
			if (QSBCore.IsHost)
			{
				Object.Instantiate(QSBNetworkManager.singleton.OrbPrefab).SpawnWithServerAuthority();
			}

			await UniTask.WaitUntil(() => TransformSync, cancellationToken: ct);
		}

		public override void OnRemoval()
		{
			if (QSBCore.IsHost)
			{
				NetworkServer.Destroy(TransformSync.gameObject);
			}
		}

		public override void SendInitialState(uint to)
		{
			if (TransformSync.hasAuthority)
			{
				this.SendMessage(new OrbDragMessage(AttachedObject._isBeingDragged) { To = to });
				var slotIndex = AttachedObject._slots.IndexOf(AttachedObject._occupiedSlot);
				this.SendMessage(new OrbSlotMessage(slotIndex, false) { To = to });
			}
		}

		public void SetDragging(bool value)
		{
			if (value == AttachedObject._isBeingDragged)
			{
				return;
			}

			DebugLog.DebugWrite($"{this} dragging = {value}");

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

			DebugLog.DebugWrite($"{this} interactible collider enabled = {AttachedObject._interactibleCollider.enabled}");
		}

		public void SetSlot(int slotIndex, bool playAudio)
		{
			var oldSlot = AttachedObject._occupiedSlot;
			var newSlot = slotIndex == -1 ? null : AttachedObject._slots[slotIndex];
			if (newSlot == oldSlot)
			{
				return;
			}

			DebugLog.DebugWrite($"{this} slot = {slotIndex} {newSlot}");

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
}
