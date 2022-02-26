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
	}

	private void OnDetectLocalLight()
	{
		if (AttachedObject._fluidDetector.InFluidType(FluidVolume.Type.WATER))
		{
			TransformSync.netIdentity.UpdateAuthQueue(AuthQueueAction.Force);
		}
	}

	public override void SendInitialState(uint to)
	{
		// todo?? SendInitialState
	}
}