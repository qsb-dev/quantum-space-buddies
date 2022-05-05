using Cysharp.Threading.Tasks;
using Mirror;
using OWML.Common;
using QSB.Messaging;
using QSB.Player;
using QSB.ShipSync.Messages;
using QSB.ShipSync.TransformSync;
using QSB.ShipSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace QSB.ShipSync;

internal class ShipManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;

	public static ShipManager Instance;

	public InteractZone HatchInteractZone;
	public HatchController HatchController;
	public ShipTractorBeamSwitch ShipTractorBeam;
	public ShipCockpitController CockpitController;
	public ShipElectricalComponent ShipElectricalComponent;
	private GameObject _shipCustomAttach;
	public uint CurrentFlyer
	{
		get => _currentFlyer;
		set
		{
			if (_currentFlyer != uint.MaxValue && value != uint.MaxValue)
			{
				DebugLog.ToConsole($"Warning - Trying to set current flyer while someone is still flying? Current:{_currentFlyer}, New:{value}", MessageType.Warning);
			}

			_currentFlyer = value;
		}
	}

	private readonly List<PlayerInfo> _playersInShip = new();

	private uint _currentFlyer = uint.MaxValue;

	public void Start()
	{
		Instance = this;
		QSBPlayerManager.OnRemovePlayer += OnRemovePlayer;
	}

	private void OnRemovePlayer(PlayerInfo player)
	{
		if (QSBCore.IsHost && player.PlayerId == CurrentFlyer)
		{
			new FlyShipMessage(false).Send();
		}
	}

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		var shipBody = Locator.GetShipBody();
		if (shipBody == null)
		{
			DebugLog.ToConsole($"Error - Couldn't find ship!", MessageType.Error);
			return;
		}

		HatchController = shipBody.GetComponentInChildren<HatchController>();
		if (HatchController == null)
		{
			DebugLog.ToConsole($"Error - Couldn't find hatch controller!", MessageType.Error);
			return;
		}

		HatchInteractZone = HatchController.GetComponent<InteractZone>();
		ShipTractorBeam = QSBWorldSync.GetUnityObject<ShipTractorBeamSwitch>();
		CockpitController = QSBWorldSync.GetUnityObject<ShipCockpitController>();
		ShipElectricalComponent = QSBWorldSync.GetUnityObject<ShipElectricalComponent>();

		var sphereShape = HatchController.GetComponent<SphereShape>();
		sphereShape.radius = 2.5f;
		sphereShape.center = new Vector3(0, 0, 1);

		if (QSBCore.IsHost)
		{
			if (ShipTransformSync.LocalInstance != null)
			{
				if (ShipTransformSync.LocalInstance.gameObject == null)
				{
					DebugLog.ToConsole($"Warning - ShipTransformSync's LocalInstance is not null, but it's gameobject is null!", MessageType.Warning);
					return;
				}

				NetworkServer.Destroy(ShipTransformSync.LocalInstance.gameObject);
			}

			if (QSBPlayerManager.LocalPlayer.TransformSync == null)
			{
				DebugLog.ToConsole($"Error - Tried to spawn ship, but LocalPlayer's TransformSync is null!", MessageType.Error);
			}

			Instantiate(QSBNetworkManager.singleton.ShipPrefab).SpawnWithServerAuthority();
		}

		QSBWorldSync.Init<QSBShipComponent, ShipComponent>();
		QSBWorldSync.Init<QSBShipHull, ShipHull>();

		_shipCustomAttach = new GameObject(nameof(ShipCustomAttach));
		_shipCustomAttach.transform.SetParent(shipBody.transform, false);
		_shipCustomAttach.AddComponent<ShipCustomAttach>();
	}

	public override void UnbuildWorldObjects() => Destroy(_shipCustomAttach);

	public void AddPlayerToShip(PlayerInfo player)
	{
		_playersInShip.Add(player);
		UpdateElectricalComponent();
	}

	public void RemovePlayerFromShip(PlayerInfo player)
	{
		_playersInShip.Remove(player);
		UpdateElectricalComponent();
	}

	public bool IsPlayerInShip(PlayerInfo player)
		=> _playersInShip.Contains(player);

	private void UpdateElectricalComponent()
	{
		var electricalSystem = ShipElectricalComponent._electricalSystem;
		var damaged = ShipElectricalComponent._damaged;

		if (_playersInShip.Count == 0)
		{
			if (!damaged)
			{
				electricalSystem.SetPowered(false);
			}
		}
		else
		{
			if (!damaged)
			{
				electricalSystem.SetPowered(true);
			}
		}
	}
}