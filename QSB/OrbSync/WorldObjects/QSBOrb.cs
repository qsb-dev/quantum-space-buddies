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
			get => TransformSync.enabled && AttachedObject._isBeingDragged;
			set
			{
				if (!TransformSync.enabled || value == IsBeingDragged)
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
	}
}
