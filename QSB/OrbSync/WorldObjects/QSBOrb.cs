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

		public bool IsBeingDragged
		{
			get => AttachedObject._isBeingDragged;
			set
			{
				if (value == IsBeingDragged)
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
		}

		public NomaiInterfaceSlot OccupiedSlot => AttachedObject._occupiedSlot;

		public void SetOccupiedSlot(NomaiInterfaceSlot slot, bool playAudio)
		{
			if (slot == OccupiedSlot)
			{
				return;
			}

			if (slot)
			{
				AttachedObject._occupiedSlot = slot;
				AttachedObject._enterSlotTime = Time.time;
				if (playAudio && AttachedObject._orbAudio != null && slot.GetPlayActivationAudio())
				{
					AttachedObject._orbAudio.PlaySlotActivatedClip();
				}

				slot._occupyingOrb = AttachedObject;
				slot.RaiseEvent(nameof(slot.OnSlotActivated), slot);
			}
			else
			{
				AttachedObject._occupiedSlot = null;

				slot._occupyingOrb = null;
				slot.RaiseEvent(nameof(slot.OnSlotDeactivated), slot);
			}
		}
	}
}
