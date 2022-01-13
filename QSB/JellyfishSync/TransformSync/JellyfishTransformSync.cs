using QSB.AuthoritySync;
using QSB.JellyfishSync.WorldObjects;
using QSB.Syncs.Unsectored.Rigidbodies;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET.Transport;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.JellyfishSync.TransformSync
{
	public class JellyfishTransformSync : UnsectoredRigidbodySync
	{
		public override bool IsReady => WorldObjectManager.AllObjectsAdded;
		public override bool UseInterpolation => false;
		public override bool IsPlayerObject => false;

		private QSBJellyfish _qsbJellyfish;
		private static readonly List<JellyfishTransformSync> _instances = new();

		protected override OWRigidbody GetRigidbody()
			=> _qsbJellyfish.AttachedObject._jellyfishBody;

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

		public override float GetNetworkSendInterval() => 10;

		protected override void Init()
		{
			_qsbJellyfish = JellyfishManager.Jellyfish[_instances.IndexOf(this)].GetWorldObject<QSBJellyfish>();
			_qsbJellyfish.TransformSync = this;

			base.Init();
			SetReferenceTransform(_qsbJellyfish.AttachedObject._planetBody.transform);

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

		public override void SerializeTransform(QNetworkWriter writer, bool initialState)
		{
			base.SerializeTransform(writer, initialState);

			if (!WorldObjectManager.AllObjectsReady)
			{
				writer.Write(false);
				return;
			}

			_qsbJellyfish.Align = true;
			writer.Write(_qsbJellyfish.IsRising);
		}

		private bool _shouldUpdate;

		public override void DeserializeTransform(QNetworkReader reader, bool initialState)
		{
			base.DeserializeTransform(reader, initialState);

			if (!WorldObjectManager.AllObjectsReady || HasAuthority)
			{
				reader.ReadBoolean();
				return;
			}

			_qsbJellyfish.Align = false;
			_qsbJellyfish.IsRising = reader.ReadBoolean();
			_shouldUpdate = true;
		}

		/// replacement using SetPosition/Rotation instead of Move
		protected override bool UpdateTransform()
		{
			if (HasAuthority)
			{
				SetValuesToSync();
				return true;
			}

			if (!_shouldUpdate)
			{
				return false;
			}

			_shouldUpdate = false;

			var hasMoved = CustomHasMoved(
				transform.position,
				_localPrevPosition,
				transform.rotation,
				_localPrevRotation,
				_relativeVelocity,
				_localPrevVelocity,
				_relativeAngularVelocity,
				_localPrevAngularVelocity);

			_localPrevPosition = transform.position;
			_localPrevRotation = transform.rotation;
			_localPrevVelocity = _relativeVelocity;
			_localPrevAngularVelocity = _relativeAngularVelocity;

			if (!hasMoved)
			{
				return true;
			}

			var pos = ReferenceTransform.FromRelPos(transform.position);
			AttachedObject.SetPosition(pos);
			AttachedObject.SetRotation(ReferenceTransform.FromRelRot(transform.rotation));
			AttachedObject.SetVelocity(ReferenceTransform.GetAttachedOWRigidbody().FromRelVel(_relativeVelocity, pos));
			AttachedObject.SetAngularVelocity(ReferenceTransform.GetAttachedOWRigidbody().FromRelAngVel(_relativeAngularVelocity));

			return true;
		}

		protected override void OnRenderObject()
		{
			if (!QSBCore.ShowLinesInDebug
			    || !WorldObjectManager.AllObjectsReady
			    || !IsReady
			    || AttachedObject == null
			    || ReferenceTransform == null
			    || AttachedObject.IsSuspended())
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
