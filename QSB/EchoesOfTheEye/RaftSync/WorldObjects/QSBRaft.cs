using Cysharp.Threading.Tasks;
using Mirror;
using QSB.AuthoritySync;
using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.EchoesOfTheEye.RaftSync.TransformSync;
using QSB.Utility.LinkedWorldObject;
using QSB.WorldSync;
using System.Linq;
using System.Threading;

namespace QSB.EchoesOfTheEye.RaftSync.WorldObjects;

public class QSBRaft : WorldObject<RaftController>, ILinkedWorldObject<RaftTransformSync>
{
	public override bool ShouldDisplayDebug() => false;

	public RaftTransformSync NetworkBehaviour { get; private set; }
	public void LinkTo(NetworkBehaviour networkBehaviour) => NetworkBehaviour = (RaftTransformSync)networkBehaviour;

	private QSBLightSensor[] _lightSensors;

	public override async UniTask Init(CancellationToken ct)
	{
		if (QSBCore.IsHost)
		{
			this.SpawnLinked(QSBNetworkManager.singleton.RaftPrefab);
		}
		else
		{
			await this.WaitForLink(ct);
		}

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
