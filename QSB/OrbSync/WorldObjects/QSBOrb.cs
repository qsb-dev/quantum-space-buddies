using QSB.OrbSync.TransformSync;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET;
using UnityEngine;

namespace QSB.OrbSync.WorldObjects
{
	public class QSBOrb : WorldObject<NomaiInterfaceOrb>
	{
		public NomaiOrbTransformSync TransformSync;

		public override void Init()
		{
			if (QSBCore.IsHost)
			{
				Object.Instantiate(QSBNetworkManager.Instance.OrbPrefab).SpawnWithServerAuthority();
			}

			StartDelayedReady();
			QSBCore.UnityEvents.RunWhen(() => TransformSync, FinishDelayedReady);
		}

		public override void OnRemoval()
		{
			if (QSBCore.IsHost)
			{
				QNetworkServer.Destroy(TransformSync.gameObject);
			}
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

		public void SetSlotted(int slotIndex, bool slotted)
		{
			var slot = AttachedObject._slots[slotIndex];
			if (slot == AttachedObject._occupiedSlot)
			{
				return;
			}

			if (slotted)
			{
				slot._occupyingOrb = AttachedObject;
				if (Time.timeSinceLevelLoad > 1f && !AttachedObject._orbBody.IsSuspended())
				{
					slot.RaiseEvent(nameof(slot.OnSlotActivated), slot);
				}

				AttachedObject._occupiedSlot = slot;
				AttachedObject._enterSlotTime = Time.time;
				if (AttachedObject._orbAudio != null && slot.GetPlayActivationAudio())
				{
					AttachedObject._orbAudio.PlaySlotActivatedClip();
				}
			}
			else
			{
				slot._occupyingOrb = null;
				if (!AttachedObject._orbBody.IsSuspended())
				{
					slot.RaiseEvent(nameof(slot.OnSlotDeactivated), slot);
				}

				AttachedObject._occupiedSlot = null;
			}
		}
	}
}
