using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.EchoesOfTheEye.RaftSync.TransformSync;
using QSB.ItemSync.WorldObjects;
using QSB.OwnershipSync;
using QSB.Utility.LinkedWorldObject;
using QSB.WorldSync;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace QSB.EchoesOfTheEye.RaftSync.WorldObjects;

public class QSBRaft : LinkedWorldObject<RaftController, RaftTransformSync>, IQSBDropTarget
{
	IItemDropTarget IQSBDropTarget.AttachedObject => AttachedObject;

	public override bool ShouldDisplayDebug() => false;

	protected override GameObject NetworkObjectPrefab => QSBNetworkManager.singleton.RaftPrefab;
	protected override bool SpawnWithServerOwnership => false;

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
		base.OnRemoval();

		foreach (var lightSensor in _lightSensors)
		{
			lightSensor.OnDetectLocalLight -= OnDetectLocalLight;
		}
	}

	private void OnDetectLocalLight()
	{
		if (AttachedObject.IsPlayerRiding())
		{
			NetworkBehaviour.netIdentity.UpdateOwnQueue(OwnQueueAction.Force);
		}
	}
}
