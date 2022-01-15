using Mirror;
using QSB.Anglerfish.WorldObjects;
using QSB.AuthoritySync;
using QSB.Syncs.Unsectored.Rigidbodies;
using QSB.WorldSync;
using QuantumUNET.Transport;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.Anglerfish.TransformSync
{
	public class AnglerTransformSync2 : UnsectoredRigidbodySync2
	{
		public override bool IsReady => WorldObjectManager.AllObjectsAdded;
		public override bool UseInterpolation => false;
		public override bool IsPlayerObject => false;

		private QSBAngler _qsbAngler;
		private static readonly List<AnglerTransformSync2> _instances = new();

		protected override OWRigidbody GetRigidbody()
			=> _qsbAngler.AttachedObject._anglerBody;

		public override void Start()
		{
			_instances.Add(this);
			base.Start();
		}

		protected override void OnDestroy()
		{
			_instances.Remove(this);
			base.OnDestroy();

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

		private bool _shouldUpdate;

		protected override void Deserialize(NetworkReader reader, bool initialState)
		{
			base.Deserialize(reader, initialState);

			if (!WorldObjectManager.AllObjectsReady)
			{
				return;
			}

			_shouldUpdate = true;
		}

		protected override bool UpdateTransform()
		{
			if (hasAuthority)
			{
				return base.UpdateTransform();
			}

			if (!_shouldUpdate)
			{
				return false;
			}

			_shouldUpdate = false;
			return base.UpdateTransform();
		}

		protected override void OnRenderObject()
		{
			if (!QSBCore.ShowLinesInDebug
			    || !WorldObjectManager.AllObjectsReady
			    || !IsReady
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
	public class AnglerTransformSync : UnsectoredRigidbodySync
	{
		public override bool IsReady => WorldObjectManager.AllObjectsAdded;
		public override bool UseInterpolation => false;
		public override bool IsPlayerObject => false;

		private QSBAngler _qsbAngler;
		private static readonly List<AnglerTransformSync> _instances = new();

		protected override OWRigidbody GetRigidbody()
			=> _qsbAngler.AttachedObject._anglerBody;

		public override void Start()
		{
			_instances.Add(this);
			base.Start();
		}

		protected override void OnDestroy()
		{
			_instances.Remove(this);
			base.OnDestroy();

			if (QSBCore.IsHost)
			{
				NetIdentity.UnregisterAuthQueue();
			}

			AttachedObject.OnUnsuspendOWRigidbody -= OnUnsuspend;
			AttachedObject.OnSuspendOWRigidbody -= OnSuspend;
		}

		public override float GetNetworkSendInterval() => 1;

		protected override void Init()
		{
			_qsbAngler = AnglerManager.Anglers[_instances.IndexOf(this)].GetWorldObject<QSBAngler>();
			// _qsbAngler.TransformSync = this;

			base.Init();
			SetReferenceTransform(_qsbAngler.AttachedObject._brambleBody.transform);

			if (QSBCore.IsHost)
			{
				NetIdentity.RegisterAuthQueue();
			}

			AttachedObject.OnUnsuspendOWRigidbody += OnUnsuspend;
			AttachedObject.OnSuspendOWRigidbody += OnSuspend;
			NetIdentity.SendAuthQueueMessage(AttachedObject.IsSuspended() ? AuthQueueAction.Remove : AuthQueueAction.Add);
		}

		private void OnUnsuspend(OWRigidbody suspendedBody) => NetIdentity.SendAuthQueueMessage(AuthQueueAction.Add);
		private void OnSuspend(OWRigidbody suspendedBody) => NetIdentity.SendAuthQueueMessage(AuthQueueAction.Remove);

		private bool _shouldUpdate;

		public override void DeserializeTransform(QNetworkReader reader, bool initialState)
		{
			base.DeserializeTransform(reader, initialState);

			if (!WorldObjectManager.AllObjectsReady || HasAuthority)
			{
				return;
			}

			_shouldUpdate = true;
		}

		protected override bool UpdateTransform()
		{
			if (HasAuthority)
			{
				return base.UpdateTransform();
			}

			if (!_shouldUpdate)
			{
				return false;
			}

			_shouldUpdate = false;
			return base.UpdateTransform();
		}

		protected override void OnRenderObject()
		{
			if (!QSBCore.ShowLinesInDebug
			    || !WorldObjectManager.AllObjectsReady
			    || !IsReady
			    || AttachedObject == null
			    || AttachedObject.IsSuspended())
			{
				return;
			}

			base.OnRenderObject();

			Popcron.Gizmos.Sphere(AttachedObject.GetPosition(), _qsbAngler.AttachedObject._arrivalDistance, Color.blue);
			Popcron.Gizmos.Sphere(AttachedObject.GetPosition(), _qsbAngler.AttachedObject._pursueDistance, Color.red);
			Popcron.Gizmos.Sphere(AttachedObject.GetPosition(), _qsbAngler.AttachedObject._escapeDistance, Color.yellow);
			Popcron.Gizmos.Sphere(AttachedObject.GetPosition()
				+ AttachedObject.transform.TransformDirection(_qsbAngler.AttachedObject._mouthOffset), 3, Color.grey);

			if (_qsbAngler.TargetTransform != null)
			{
				Popcron.Gizmos.Line(_qsbAngler.TargetTransform.position, AttachedObject.GetPosition(), Color.gray);
				Popcron.Gizmos.Line(_qsbAngler.TargetTransform.position, _qsbAngler.TargetTransform.position + _qsbAngler.TargetVelocity, Color.green);
				Popcron.Gizmos.Line(AttachedObject.GetPosition(), _qsbAngler.AttachedObject._targetPos, Color.red);
				Popcron.Gizmos.Sphere(_qsbAngler.AttachedObject._targetPos, 5, Color.red);
			}

			// Popcron.Gizmos.Line(AttachedObject.GetPosition(), _qsbAngler.AttachedObject.GetTargetPosition(), Color.white);
		}
	}
}
