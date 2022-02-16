using QSB.Player.TransformSync;
using QSB.ShipSync.TransformSync;
using QSB.Syncs.Unsectored.Rigidbodies;
using QSB.Tools.ProbeTool.TransformSync;
using QSB.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.Syncs.Occasional
{
	public class OccasionalTransformSync : UnsectoredRigidbodySync
	{
		protected override bool UseInterpolation => false;
		protected override bool OnlyApplyOnDeserialize => true;

		protected override OWRigidbody InitAttachedRigidbody() => OccasionalManager.Bodies[_instances.IndexOf(this)].Body;

		private static readonly List<OccasionalTransformSync> _instances = new();

		public override void OnStartClient()
		{
			_instances.Add(this);
			base.OnStartClient();
		}

		public override void OnStopClient()
		{
			_instances.Remove(this);
			base.OnStopClient();
		}

		private Sector[] _sectors;
		private OWRigidbody[] _childBodies;

		protected override float SendInterval => 20;
		protected override bool UseReliableRpc => true;

		protected override void Init()
		{
			base.Init();
			SetReferenceTransform(OccasionalManager.Bodies[_instances.IndexOf(this)].RefBody.transform);

			_sectors = SectorManager.s_sectors
				.Where(x => x._attachedOWRigidbody == AttachedRigidbody).ToArray();
			_childBodies = CenterOfTheUniverse.s_rigidbodies
				.Where(x => x._origParentBody == AttachedRigidbody).ToArray();
		}

		protected override void ApplyToAttached()
		{
			if (_sectors.Contains(PlayerTransformSync.LocalInstance?.ReferenceSector?.AttachedObject))
			{
				QueueMove(Locator._playerBody);
			}

			if (_sectors.Contains(ShipTransformSync.LocalInstance?.ReferenceSector?.AttachedObject))
			{
				QueueMove(Locator._shipBody);
			}

			if (_sectors.Contains(PlayerProbeSync.LocalInstance?.ReferenceSector?.AttachedObject))
			{
				QueueMove(Locator._probe._owRigidbody);
			}

			foreach (var child in _childBodies)
			{
				QueueMove(child);
			}

			var pos = ReferenceTransform.FromRelPos(transform.position);
			AttachedRigidbody.SetPosition(pos);
			AttachedRigidbody.SetRotation(ReferenceTransform.FromRelRot(transform.rotation));
			AttachedRigidbody.SetVelocity(ReferenceRigidbody.FromRelVel(_relativeVelocity, pos));
			AttachedRigidbody.SetAngularVelocity(ReferenceRigidbody.FromRelAngVel(_relativeAngularVelocity));

			Move();
		}

		private readonly List<MoveData> _toMove = new();

		private struct MoveData
		{
			public OWRigidbody Child;
			public Vector3 RelPos;
			public Quaternion RelRot;
			public Vector3 RelVel;
			public Vector3 RelAngVel;
		}

		private void QueueMove(OWRigidbody child)
		{
			if (!child)
			{
				return; // wtf
			}

			if (child.transform.parent)
			{
				// it's parented to AttachedObject or one of its children
				return;
			}

			var pos = child.GetPosition();
			_toMove.Add(new MoveData
			{
				Child = child,
				RelPos = AttachedRigidbody.transform.ToRelPos(pos),
				RelRot = AttachedRigidbody.transform.ToRelRot(child.GetRotation()),
				RelVel = AttachedRigidbody.ToRelVel(child.GetVelocity(), pos),
				RelAngVel = AttachedRigidbody.ToRelAngVel(child.GetAngularVelocity())
			});
		}

		private void Move()
		{
			foreach (var data in _toMove)
			{
				var pos = AttachedRigidbody.transform.FromRelPos(data.RelPos);
				data.Child.SetPosition(pos);
				data.Child.SetRotation(AttachedRigidbody.transform.FromRelRot(data.RelRot));
				data.Child.SetVelocity(AttachedRigidbody.FromRelVel(data.RelVel, pos));
				data.Child.SetAngularVelocity(AttachedRigidbody.FromRelAngVel(data.RelAngVel));
			}

			_toMove.Clear();
		}
	}
}
