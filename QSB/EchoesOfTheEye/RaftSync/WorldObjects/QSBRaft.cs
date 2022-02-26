using Cysharp.Threading.Tasks;
using Mirror;
using QSB.AuthoritySync;
using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.EchoesOfTheEye.RaftSync.TransformSync;
using QSB.WorldSync;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace QSB.EchoesOfTheEye.RaftSync.WorldObjects;

public class QSBRaft : WorldObject<RaftController>
{
	public override bool ShouldDisplayDebug() => false;

	public RaftTransformSync TransformSync;

	public override async UniTask Init(CancellationToken ct)
	{
		if (QSBCore.IsHost)
		{
			NetworkServer.Spawn(Object.Instantiate(QSBNetworkManager.singleton.RaftPrefab));
		}

		await UniTask.WaitUntil(() => TransformSync, cancellationToken: ct);

		foreach (var lightSensor in AttachedObject._lightSensors)
		{
			lightSensor.OnDetectLight += OnDetectLight;
		}
	}

	public override void OnRemoval()
	{
		if (QSBCore.IsHost)
		{
			NetworkServer.Destroy(TransformSync.gameObject);
		}

		foreach (var lightSensor in AttachedObject._lightSensors)
		{
			lightSensor.OnDetectLight -= OnDetectLight;
		}
	}

	private void OnDetectLight()
	{
		if (!AttachedObject._fluidDetector.InFluidType(FluidVolume.Type.WATER))
		{
			return;
		}

		if (!AttachedObject._lightSensors.Any(x => x.GetWorldObject<QSBSingleLightSensor>().IlluminatedByLocalPlayer))
		{
			return;
		}

		TransformSync.netIdentity.UpdateAuthQueue(AuthQueueAction.Force);
	}

	public override void SendInitialState(uint to)
	{
		// todo?? SendInitialState
	}
}