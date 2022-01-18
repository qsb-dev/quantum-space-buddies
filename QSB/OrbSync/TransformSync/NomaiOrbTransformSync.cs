using OWML.Common;
using QSB.AuthoritySync;
using QSB.OrbSync.WorldObjects;
using QSB.Syncs.Unsectored.Transforms;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.OrbSync.TransformSync
{
	public class NomaiOrbTransformSync : UnsectoredTransformSync
	{
		protected override bool IsReady => QSBWorldSync.AllObjectsAdded;
		protected override bool UseInterpolation => true;
		protected override float DistanceLeeway => 1f;

		protected override Transform InitLocalTransform() => _qsbOrb.AttachedObject.transform;
		protected override Transform InitRemoteTransform() => _qsbOrb.AttachedObject.transform;

		private OWRigidbody _attachedBody;
		private QSBOrb _qsbOrb;
		private static readonly List<NomaiOrbTransformSync> _instances = new();

		public override void OnStartClient()
		{
			_instances.Add(this);
			base.OnStartClient();
		}

		protected override void OnDestroy()
		{
			_instances.Remove(this);
			base.OnDestroy();

			if (QSBCore.IsHost)
			{
				netIdentity.UnregisterAuthQueue();
			}

			_attachedBody.OnUnsuspendOWRigidbody -= OnUnsuspend;
			_attachedBody.OnSuspendOWRigidbody -= OnSuspend;
		}

		protected override void Init()
		{
			var index = _instances.IndexOf(this);
			if (!OrbManager.Orbs.TryGet(index, out var orb))
			{
				DebugLog.ToConsole($"Error - No orb at index {index}.", MessageType.Error);
				return;
			}

			_qsbOrb = orb.GetWorldObject<QSBOrb>();
			_qsbOrb.TransformSync = this;

			base.Init();
			_attachedBody = AttachedTransform.GetAttachedOWRigidbody();
			SetReferenceTransform(_attachedBody.GetOrigParent());

			/*
			if (_attachedBody.GetOrigParent() == Locator.GetRootTransform())
			{
				DebugLog.DebugWrite($"{LogName} with AttachedObject {AttachedObject.name} had it's original parent as SolarSystemRoot - Disabling...");
				enabled = false;
				return;
			}
			*/

			if (QSBCore.IsHost)
			{
				netIdentity.RegisterAuthQueue();
			}

			_attachedBody.OnUnsuspendOWRigidbody += OnUnsuspend;
			_attachedBody.OnSuspendOWRigidbody += OnSuspend;
			netIdentity.SendAuthQueueMessage(_attachedBody.IsSuspended() ? AuthQueueAction.Remove : AuthQueueAction.Add);
		}

		private void OnUnsuspend(OWRigidbody suspendedBody) => netIdentity.SendAuthQueueMessage(AuthQueueAction.Add);
		private void OnSuspend(OWRigidbody suspendedBody) => netIdentity.SendAuthQueueMessage(AuthQueueAction.Remove);
	}
}
