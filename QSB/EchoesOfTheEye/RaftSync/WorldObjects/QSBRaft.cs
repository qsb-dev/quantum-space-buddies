using Cysharp.Threading.Tasks;
using Mirror;
using QSB.AuthoritySync;
using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.EchoesOfTheEye.RaftSync.TransformSync;
using QSB.Utility.LinkedWorldObject;
using QSB.WorldSync;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace QSB.EchoesOfTheEye.RaftSync.WorldObjects;

public class QSBRaft : LinkedWorldObject<RaftController, RaftTransformSync>
{
	public override bool ShouldDisplayDebug() => false;

	protected override GameObject NetworkObjectPrefab => QSBNetworkManager.singleton.RaftPrefab;
	protected override bool SpawnWithServerAuthority => false;

	private QSBLightSensor[] _lightSensors;

	public override async UniTask Init(CancellationToken ct)
	{
		await base.Init(ct);

		await UniTask.WaitUntil(() => QSBWorldSync.AllObjectsAdded, cancellationToken: ct);
		_lightSensors = AttachedObject._lightSensors.Select(x => x.GetWorldObject<QSBLightSensor>()).ToArray();

		foreach (var lightSensor in _lightSensors)
		{
			lightSensor.OnDetectLocalLight += OnDetectLocalLight;
		}
	}

	public override void OnRemoval()
	{
		if (QSBCore.IsHost)
		{
			NetworkServer.Destroy(NetworkBehaviour.gameObject);
		}

		foreach (var lightSensor in _lightSensors)
		{
			lightSensor.OnDetectLocalLight -= OnDetectLocalLight;
		}
	}

	private void OnDetectLocalLight()
	{
		if (AttachedObject.IsPlayerRiding())
		{
			NetworkBehaviour.netIdentity.UpdateAuthQueue(AuthQueueAction.Force);
		}
	}

	public override void SendInitialState(uint to)
	{
		// not really needed. things work fine without it
	}
}
