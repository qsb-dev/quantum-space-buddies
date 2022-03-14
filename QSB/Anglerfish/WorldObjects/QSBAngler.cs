using Cysharp.Threading.Tasks;
using Mirror;
using QSB.Anglerfish.Messages;
using QSB.Anglerfish.TransformSync;
using QSB.Messaging;
using QSB.Utility.LinkedWorldObject;
using QSB.WorldSync;
using System.Threading;
using UnityEngine;

namespace QSB.Anglerfish.WorldObjects;

public class QSBAngler : WorldObject<AnglerfishController>, ILinkedWorldObject<AnglerTransformSync>
{
	public override bool ShouldDisplayDebug() => false;

	public Transform TargetTransform;
	public Vector3 TargetVelocity { get; private set; }

	private Vector3 _lastTargetPosition;

	public AnglerTransformSync NetworkBehaviour { get; private set; }
	public void LinkTo(NetworkBehaviour networkBehaviour) => NetworkBehaviour = (AnglerTransformSync)networkBehaviour;

	public override async UniTask Init(CancellationToken ct)
	{
		if (QSBCore.IsHost)
		{
			this.SpawnLinked(QSBNetworkManager.singleton.AnglerPrefab);
		}
		else
		{
			await this.WaitForLink(ct);
		}
	}

	public override void OnRemoval()
	{
		if (QSBCore.IsHost)
		{
			NetworkServer.Destroy(NetworkBehaviour.gameObject);
		}
	}

	public override void SendInitialState(uint to) =>
		this.SendMessage(new AnglerDataMessage(this) { To = to });

	public void UpdateTargetVelocity()
	{
		if (TargetTransform == null)
		{
			return;
		}

		TargetVelocity = TargetTransform.position - _lastTargetPosition;
		_lastTargetPosition = TargetTransform.position;
	}
}