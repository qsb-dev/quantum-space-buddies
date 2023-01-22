using QSB.Player.TransformSync;
using QSB.ShipSync.TransformSync;
using QSB.Syncs.Unsectored.Rigidbodies;
using QSB.Tools.ProbeTool.TransformSync;
using QSB.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.Syncs.Occasional;

public class OccasionalTransformSync : UnsectoredRigidbodySync
{
	protected override bool UseInterpolation => false;

	protected override OWRigidbody InitAttachedRigidbody() => OccasionalManager.Bodies[Instances.IndexOf(this)].Body;

	public static readonly List<OccasionalTransformSync> Instances = new();

	public override void OnStartClient()
	{
		Instances.Add(this);
		base.OnStartClient();
	}

	public override void OnStopClient()
	{
		Instances.Remove(this);
		base.OnStopClient();
	}

	private Sector[] _sectors;
	private OWRigidbody[] _childBodies;

	protected override float SendInterval => 20;
	protected override bool UseReliableRpc => true;

	protected override void Init()
	{
		base.Init();
		SetReferenceTransform(OccasionalManager.Bodies[Instances.IndexOf(this)].RefBody.transform);

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
		AttachedRigidbody.SetVelocity(ReferenceRigidbody.FromRelVel(Velocity, pos));
		AttachedRigidbody.SetAngularVelocity(ReferenceRigidbody.FromRelAngVel(AngularVelocity));

		Move();

		if (!Physics.autoSyncTransforms)
		{
			Physics.SyncTransforms();
		}
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
			RelPos = AttachedTransform.ToRelPos(pos),
			RelRot = AttachedTransform.ToRelRot(child.GetRotation()),
			RelVel = AttachedRigidbody.ToRelVel(child.GetVelocity(), pos),
			RelAngVel = AttachedRigidbody.ToRelAngVel(child.GetAngularVelocity())
		});
	}

	private void Move()
	{
		foreach (var data in _toMove)
		{
			var pos = AttachedTransform.FromRelPos(data.RelPos);
			data.Child.SetPosition(pos);
			data.Child.SetRotation(AttachedTransform.FromRelRot(data.RelRot));
			data.Child.SetVelocity(AttachedRigidbody.FromRelVel(data.RelVel, pos));
			data.Child.SetAngularVelocity(AttachedRigidbody.FromRelAngVel(data.RelAngVel));
		}

		_toMove.Clear();
	}
}
