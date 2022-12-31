using Cysharp.Threading.Tasks;
using Mirror;
using OWML.Common;
using QSB.ModelShip.TransformSync;
using QSB.Utility;
using QSB.WorldSync;
using System.Threading;

namespace QSB.ModelShip;

internal class ModelShipManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;
	public override bool DlcOnly => false;

	public static ModelShipManager Instance;

	public uint CurrentFlyer
	{
		get => _currentFlyer;
		set
		{
			if (_currentFlyer != uint.MaxValue && value != uint.MaxValue)
			{
				DebugLog.ToConsole($"Warning - Trying to set current model ship flyer while someone is still flying? Current:{_currentFlyer}, New:{value}", MessageType.Warning);
			}

			_currentFlyer = value;
		}
	}
	private uint _currentFlyer = uint.MaxValue;

	public void Start()
	{
		Instance = this;
	}

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		// NH can remove this
		var modelShip = QSBWorldSync.GetUnityObject<RemoteFlightConsole>()._modelShipBody;
		if (!modelShip)
		{
			return;
		}

		if (QSBCore.IsHost)
		{
			Instantiate(QSBNetworkManager.singleton.ModelShipPrefab).SpawnWithServerAuthority();
		}

		// Is 0 by default -> 2D (bad)
		QSBWorldSync.GetUnityObject<RemoteFlightConsole>()._consoleAudio.spatialBlend = 1;
	}

	public override void UnbuildWorldObjects()
	{
		if (QSBCore.IsHost)
		{
			if (ModelShipTransformSync.LocalInstance != null)
			{
				if (ModelShipTransformSync.LocalInstance.gameObject == null)
				{
					DebugLog.ToConsole($"Warning - ShipTransformSync's LocalInstance is not null, but it's gameobject is null!", MessageType.Warning);
					return;
				}

				NetworkServer.Destroy(ModelShipTransformSync.LocalInstance.gameObject);
			}
		}
	}
}
