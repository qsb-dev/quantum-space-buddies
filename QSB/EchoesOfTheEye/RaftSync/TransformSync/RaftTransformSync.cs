using QSB.AuthoritySync;
using QSB.EchoesOfTheEye.RaftSync.WorldObjects;
using QSB.Syncs.Unsectored.Rigidbodies;
using QSB.WorldSync;
using System.Collections.Generic;

namespace QSB.EchoesOfTheEye.RaftSync.TransformSync;

public class RaftTransformSync : UnsectoredRigidbodySync
{
	protected override bool UseInterpolation => false;
	protected override bool OnlyApplyOnDeserialize => true;

	private QSBRaft _qsbRaft;
	private static readonly List<RaftTransformSync> _instances = new();

	protected override OWRigidbody InitAttachedRigidbody() => _qsbRaft.AttachedObject._raftBody;

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
		_qsbRaft = RaftManager.Rafts[_instances.IndexOf(this)].GetWorldObject<QSBRaft>();
		_qsbRaft.TransformSync = this;

		base.Init();
		SetReferenceTransform(AttachedRigidbody.GetOrigParent());

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
}
