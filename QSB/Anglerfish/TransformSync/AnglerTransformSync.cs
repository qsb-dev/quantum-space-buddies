using QSB.Anglerfish.WorldObjects;
using QSB.AuthoritySync;
using QSB.Syncs.Unsectored.Rigidbodies;
using QSB.WorldSync;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.Anglerfish.TransformSync
{
	public class AnglerTransformSync : UnsectoredRigidbodySync
	{
		protected override bool UseInterpolation => false;
		protected override bool OnlyApplyOnDeserialize => true;

		private QSBAngler _qsbAngler;
		private static readonly List<AnglerTransformSync> _instances = new();

		protected override OWRigidbody InitAttachedRigidbody()
			=> _qsbAngler.AttachedObject._anglerBody;

		public override void OnStartClient()
		{
			_instances.Add(this);
			base.OnStartClient();
		}

		public override void OnStopClient()
		{
			_instances.Remove(this);
			base.OnStopClient();

			if (QSBCore.IsHost)
			{
				netIdentity.UnregisterAuthQueue();
			}

			AttachedRigidbody.OnUnsuspendOWRigidbody -= OnUnsuspend;
			AttachedRigidbody.OnSuspendOWRigidbody -= OnSuspend;
		}

		protected override float SendInterval => 1;
		protected override bool UseReliableRpc => true;

		protected override void Init()
		{
			_qsbAngler = AnglerManager.Anglers[_instances.IndexOf(this)].GetWorldObject<QSBAngler>();
			_qsbAngler.TransformSync = this;

			base.Init();
			SetReferenceTransform(_qsbAngler.AttachedObject._brambleBody.transform);

			if (QSBCore.IsHost)
			{
				netIdentity.RegisterAuthQueue();
			}

			AttachedRigidbody.OnUnsuspendOWRigidbody += OnUnsuspend;
			AttachedRigidbody.OnSuspendOWRigidbody += OnSuspend;
			netIdentity.SendAuthQueueMessage(AttachedRigidbody.IsSuspended() ? AuthQueueAction.Remove : AuthQueueAction.Add);
		}

		private void OnUnsuspend(OWRigidbody suspendedBody) => netIdentity.SendAuthQueueMessage(AuthQueueAction.Add);
		private void OnSuspend(OWRigidbody suspendedBody) => netIdentity.SendAuthQueueMessage(AuthQueueAction.Remove);

		protected override void OnRenderObject()
		{
			if (!QSBCore.ShowLinesInDebug
				|| !IsInitialized
				|| AttachedRigidbody == null
				|| AttachedRigidbody.IsSuspended())
			{
				return;
			}

			base.OnRenderObject();

			Popcron.Gizmos.Sphere(AttachedRigidbody.GetPosition(), _qsbAngler.AttachedObject._arrivalDistance, Color.blue);
			Popcron.Gizmos.Sphere(AttachedRigidbody.GetPosition(), _qsbAngler.AttachedObject._pursueDistance, Color.red);
			Popcron.Gizmos.Sphere(AttachedRigidbody.GetPosition(), _qsbAngler.AttachedObject._escapeDistance, Color.yellow);
			Popcron.Gizmos.Sphere(AttachedRigidbody.GetPosition()
				+ AttachedRigidbody.transform.TransformDirection(_qsbAngler.AttachedObject._mouthOffset), 3, Color.grey);

			if (_qsbAngler.TargetTransform != null)
			{
				Popcron.Gizmos.Line(_qsbAngler.TargetTransform.position, AttachedRigidbody.GetPosition(), Color.gray);
				Popcron.Gizmos.Line(_qsbAngler.TargetTransform.position, _qsbAngler.TargetTransform.position + _qsbAngler.TargetVelocity, Color.green);
				Popcron.Gizmos.Line(AttachedRigidbody.GetPosition(), _qsbAngler.AttachedObject._targetPos, Color.red);
				Popcron.Gizmos.Sphere(_qsbAngler.AttachedObject._targetPos, 5, Color.red);
			}

			// Popcron.Gizmos.Line(AttachedObject.GetPosition(), _qsbAngler.AttachedObject.GetTargetPosition(), Color.white);
		}
	}
}
