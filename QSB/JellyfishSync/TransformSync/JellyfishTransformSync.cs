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
		protected override bool IsReady => QSBWorldSync.AllObjectsAdded;
		protected override bool UseInterpolation => false;
		protected override bool OnlyApplyOnDeserialize => true;

		private QSBJellyfish _qsbJellyfish;
		private static readonly List<JellyfishTransformSync> _instances = new();

		protected override OWRigidbody InitAttachedRigidbody()
			=> _qsbJellyfish.AttachedObject._jellyfishBody;

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

			AttachedRigidbody.OnUnsuspendOWRigidbody -= OnUnsuspend;
			AttachedRigidbody.OnSuspendOWRigidbody -= OnSuspend;
		}

		protected override float SendInterval => 10;
		protected override bool UseReliableRpc => true;

		protected override void Init()
		{
			_qsbJellyfish = JellyfishManager.Jellyfish[_instances.IndexOf(this)].GetWorldObject<QSBJellyfish>();
			_qsbJellyfish.TransformSync = this;

			base.Init();
			SetReferenceTransform(_qsbJellyfish.AttachedObject._planetBody.transform);

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

		private bool _isRising;

		protected override void Serialize(NetworkWriter writer, bool initialState)
		{
			base.Serialize(writer, initialState);

			writer.Write(_isRising);
		}

		protected override void Deserialize(NetworkReader reader, bool initialState)
		{
			base.Deserialize(reader, initialState);

			_isRising = reader.ReadBool();
		}

		protected override void GetFromAttached()
		{
			base.GetFromAttached();

			_qsbJellyfish.Align = true;
			_isRising = _qsbJellyfish.IsRising;
		}

		/// replacement using SetPosition/Rotation instead of Move
		protected override void ApplyToAttached()
		{
			_qsbJellyfish.Align = false;
			_qsbJellyfish.IsRising = _isRising;

			var pos = ReferenceTransform.FromRelPos(transform.position);
			AttachedRigidbody.SetPosition(pos);
			AttachedRigidbody.SetRotation(ReferenceTransform.FromRelRot(transform.rotation));
			AttachedRigidbody.SetVelocity(ReferenceTransform.GetAttachedOWRigidbody().FromRelVel(_relativeVelocity, pos));
			AttachedRigidbody.SetAngularVelocity(ReferenceTransform.GetAttachedOWRigidbody().FromRelAngVel(_relativeAngularVelocity));
		}

		protected override void OnRenderObject()
		{
			if (!QSBCore.ShowLinesInDebug
				|| !IsInitialized
				|| AttachedRigidbody == null
				|| ReferenceTransform == null
				|| AttachedRigidbody.IsSuspended())
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
