using Cysharp.Threading.Tasks;
using Mirror;
using QSB.AuthoritySync;
using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.EchoesOfTheEye.RaftSync.Messages;
using QSB.EchoesOfTheEye.RaftSync.TransformSync;
using QSB.Messaging;
using QSB.WorldSync;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace QSB.EchoesOfTheEye.RaftSync.WorldObjects;

public class QSBRaft : WorldObject<RaftController>
{
	public override bool ShouldDisplayDebug() => false;

	public RaftTransformSync TransformSync;

	private QSBLightSensor[] _lightSensors;

	public override async UniTask Init(CancellationToken ct)
	{
		if (QSBCore.IsHost)
		{
			NetworkServer.Spawn(Object.Instantiate(QSBNetworkManager.singleton.RaftPrefab));
		}

		await UniTask.WaitUntil(() => TransformSync, cancellationToken: ct);

		await UniTask.WaitUntil(() => QSBWorldSync.AllObjectsAdded, cancellationToken: ct);
		_lightSensors = AttachedObject._lightSensors.Select(x => x.GetWorldObject<QSBLightSensor>()).ToArray();

		foreach (var lightSensor in _lightSensors)
		{
			lightSensor.OnDetectLocalLight += OnDetectLocalLight;
		}
	}

	private readonly CancellationTokenSource _cts = new();

	public override void OnRemoval()
	{
		if (QSBCore.IsHost)
		{
			NetworkServer.Destroy(TransformSync.gameObject);
		}

		foreach (var lightSensor in _lightSensors)
		{
			lightSensor.OnDetectLocalLight -= OnDetectLocalLight;
		}

		_cts.Cancel();
	}

	private void OnDetectLocalLight()
	{
		if (AttachedObject.IsPlayerRiding())
		{
			TransformSync.netIdentity.UpdateAuthQueue(AuthQueueAction.Force);
		}
	}

	public override void SendInitialState(uint to)
	{
		if (RaftManager.DamRaftLift._raft == AttachedObject)
		{
			new StartLiftingRaftMessage(this).Send();
		}
		else
		{
			this.SendMessage(new RaftSetDockMessage(AttachedObject._dock));
		}
	}

	public void SetDock(RaftDock raftDock)
	{
		if (raftDock == AttachedObject._dock)
		{
			return;
		}

		// undock from current dock
		if (AttachedObject._dock != null)
		{
			// todo
		}

		// dock to new dock
		if (raftDock != null)
		{
			// todo
		}
	}
}
