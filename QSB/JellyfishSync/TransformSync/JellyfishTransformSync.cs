using Mirror;
using QSB.AuthoritySync;
using QSB.JellyfishSync.WorldObjects;
using QSB.Syncs.Unsectored.Rigidbodies;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.JellyfishSync.TransformSync
{
	public class JellyfishTransformSync : UnsectoredRigidbodySync
	{
		protected override bool UseInterpolation => false;
		protected override bool AllowInactiveAttachedObject => true; // since they deactivate when suspended

		private QSBJellyfish _qsbJellyfish;
		private bool _isRising;
		private static readonly List<JellyfishTransformSync> _instances = new();

		protected override OWRigidbody InitAttachedRigidbody()
			=> _qsbJellyfish.AttachedObject._jellyfishBody;

		public override void OnStartClient()
		{
			_instances.Add(this);
			if (QSBCore.IsHost)
			{
				netIdentity.RegisterAuthQueue();
			}

			base.OnStartClient();
		}

		public override void OnStopClient()
		{
			_instances.Remove(this);
			if (QSBCore.IsHost)
			{
				netIdentity.UnregisterAuthQueue();
			}

			base.OnStopClient();
		}

		protected override float SendInterval => 10;
		protected override bool UseReliableRpc => true;

		protected override void Init()
		{
			_qsbJellyfish = JellyfishManager.Jellyfish[_instances.IndexOf(this)].GetWorldObject<QSBJellyfish>();
			_qsbJellyfish.TransformSync = this;

			base.Init();
			SetReferenceTransform(_qsbJellyfish.AttachedObject._planetBody.transform);

			AttachedRigidbody.OnUnsuspendOWRigidbody += OnUnsuspend;
			AttachedRigidbody.OnSuspendOWRigidbody += OnSuspend;
			netIdentity.UpdateAuthQueue(AttachedRigidbody.IsSuspended() ? AuthQueueAction.Remove : AuthQueueAction.Add);
		}

		protected override void Uninit()
		{
			base.Uninit();

			AttachedRigidbody.OnUnsuspendOWRigidbody -= OnUnsuspend;
			AttachedRigidbody.OnSuspendOWRigidbody -= OnSuspend;
		}

		private void OnUnsuspend(OWRigidbody suspendedBody) => netIdentity.UpdateAuthQueue(AuthQueueAction.Add);
		private void OnSuspend(OWRigidbody suspendedBody) => netIdentity.UpdateAuthQueue(AuthQueueAction.Remove);

		protected override void Serialize(NetworkWriter writer)
		{
			writer.Write(_isRising);
			base.Serialize(writer);
		}

		protected override void Deserialize(NetworkReader reader)
		{
			_isRising = reader.Read<bool>();
			base.Deserialize(reader);
		}

		protected override void GetFromAttached()
		{
			base.GetFromAttached();

			_isRising = _qsbJellyfish.AttachedObject._isRising;
		}

		/// replacement using SetPosition/Rotation instead of Move
		protected override void ApplyToAttached()
		{
			var pos = ReferenceTransform.FromRelPos(transform.position);
			AttachedRigidbody.SetPosition(pos);
			AttachedRigidbody.SetRotation(ReferenceTransform.FromRelRot(transform.rotation));
			AttachedRigidbody.SetVelocity(ReferenceRigidbody.FromRelVel(Velocity, pos));
			AttachedRigidbody.SetAngularVelocity(ReferenceRigidbody.FromRelAngVel(AngularVelocity));

			_qsbJellyfish.SetIsRising(_isRising);
		}

		protected override void OnRenderObject()
		{
			if (!QSBCore.DebugSettings.DrawLines
			    || !IsValid
			    || !ReferenceTransform
			    || !AttachedTransform.gameObject.activeInHierarchy)
			{
				return;
			}

			base.OnRenderObject();

			var jellyfish = _qsbJellyfish.AttachedObject;
			var position = ReferenceTransform.position;
			var dir = Vector3.Normalize(jellyfish.transform.position - position);
			// Popcron.Gizmos.Line(position + dir * jellyfish._lowerLimit, position + dir * jellyfish._upperLimit, Color.magenta);
			Popcron.Gizmos.Sphere(position + dir * jellyfish._lowerLimit, 10f, Color.magenta);
			Popcron.Gizmos.Sphere(position + dir * jellyfish._upperLimit, 10f, Color.magenta);
		}
	}
}