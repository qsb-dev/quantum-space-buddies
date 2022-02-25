using QSB.AuthoritySync;
using QSB.OrbSync.WorldObjects;
using QSB.Syncs.Unsectored.Transforms;
using QSB.WorldSync;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.OrbSync.TransformSync;

public class NomaiOrbTransformSync : UnsectoredTransformSync
{
	/// <summary>
	/// normally prints error when attached object is null.
	/// this overrides it so that doesn't happen, since the orb can be destroyed.
	/// </summary>
	protected override bool CheckValid() => AttachedTransform && base.CheckValid();

	protected override bool UseInterpolation => true;
	protected override float DistanceChangeThreshold => 1f;

	protected override Transform InitLocalTransform() => _qsbOrb.AttachedObject.transform;
	protected override Transform InitRemoteTransform() => _qsbOrb.AttachedObject.transform;

	private QSBOrb _qsbOrb;
	private static readonly List<NomaiOrbTransformSync> _instances = new();

	public override void OnStartClient()
	{
		_instances.Add(this);
		if (QSBCore.IsHost)
		{
			netIdentity.RegisterAuthQueue(true);
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

	protected override void Init()
	{
		_qsbOrb = OrbManager.Orbs[_instances.IndexOf(this)].GetWorldObject<QSBOrb>();
		_qsbOrb.TransformSync = this;

		base.Init();
		var body = AttachedTransform.GetAttachedOWRigidbody();
		SetReferenceTransform(body.GetOrigParent());

		body.OnUnsuspendOWRigidbody += OnUnsuspend;
		body.OnSuspendOWRigidbody += OnSuspend;
		netIdentity.UpdateAuthQueue(body.IsSuspended() ? AuthQueueAction.Remove : AuthQueueAction.Add);
	}

	protected override void Uninit()
	{
		base.Uninit();

		// this is null sometimes on here, but not on other similar transforms syncs (like anglers)
		// idk why, but whatever
		if (AttachedTransform)
		{
			var body = AttachedTransform.GetAttachedOWRigidbody();
			if (body)
			{
				body.OnUnsuspendOWRigidbody -= OnUnsuspend;
				body.OnSuspendOWRigidbody -= OnSuspend;
			}
		}
	}

	private void OnUnsuspend(OWRigidbody suspendedBody) => netIdentity.UpdateAuthQueue(AuthQueueAction.Add);
	private void OnSuspend(OWRigidbody suspendedBody) => netIdentity.UpdateAuthQueue(AuthQueueAction.Remove);

	protected override void ApplyToAttached()
	{
		base.ApplyToAttached();

		_qsbOrb.AttachedObject.SetTargetPosition(AttachedTransform.position);
	}
}