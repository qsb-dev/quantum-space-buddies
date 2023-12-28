using Cysharp.Threading.Tasks;
using OWML.Common;
using QSB.SectorSync.WorldObjects;
using QSB.Syncs.Sectored;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace QSB.SectorSync;

public class QSBSectorManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.Both;

	public static QSBSectorManager Instance { get; private set; }
	private bool _isReady;

	public readonly List<BaseSectoredSync> SectoredSyncs = new();

	private const float UpdateInterval = 0.4f;
	private float _timer = UpdateInterval;

	private void Update()
	{
		_timer += Time.unscaledDeltaTime;
		if (_timer < UpdateInterval)
		{
			return;
		}

		_timer = 0;
		UpdateReferenceSectors();
	}

	public void UpdateReferenceSectors()
	{
		if (!Instance._isReady || !QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		foreach (var sync in SectoredSyncs)
		{
			if (sync.isOwned
				&& sync.IsValid
				&& sync.AttachedTransform.gameObject.activeInHierarchy)
			{
				UpdateReferenceSector(sync);
			}
		}
	}

	private static void UpdateReferenceSector(BaseSectoredSync sync)
	{
		var closestSector = sync.SectorDetector.GetClosestSector();
		if (closestSector == null)
		{
			return;
		}

		sync.SetReferenceSector(closestSector);
	}

	public void Awake()
	{
		Instance = this;
		DebugLog.DebugWrite("Sector Manager ready.", MessageType.Success);
	}

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		DebugLog.DebugWrite("Building sectors...", MessageType.Info);
		this.Try("creating fake sectors", CreateFakeSectors);

		QSBWorldSync.Init<QSBSector, Sector>();
		_isReady = QSBWorldSync.GetWorldObjects<QSBSector>().Any();
	}

	public override void UnbuildWorldObjects() =>
		_isReady = false;

	private static void CreateFakeSectors()
	{
		if (QSBSceneManager.CurrentScene != OWScene.SolarSystem)
		{
			return;
		}

		// time loop spinning ring
		{
			var TimeLoopRing_Body = GameObject.Find("TimeLoopRing_Body");
			var Sector_TimeLoopInterior = GameObject.Find("Sector_TimeLoopInterior").GetComponent<Sector>();
			// use the same trigger as the parent sector
			FakeSector.Create(TimeLoopRing_Body, Sector_TimeLoopInterior,
				x => x._triggerRoot = Sector_TimeLoopInterior._triggerRoot);
		}

		// TH elevators
		foreach (var elevator in QSBWorldSync.GetUnityObjects<Elevator>())
		{
			FakeSector.Create(elevator.gameObject,
				elevator.GetComponentInParent<Sector>(),
				x => x._triggerRoot = elevator.gameObject);
		}

		// rafts
		foreach (var raft in QSBWorldSync.GetUnityObjects<RaftController>())
		{
			FakeSector.Create(raft.gameObject,
				raft._sector,
				x => x._triggerRoot = raft._rideVolume.gameObject);
		}

		// todo black hole forge

		// cage elevators
		foreach (var cageElevator in QSBWorldSync.GetUnityObjects<CageElevator>())
		{
			FakeSector.Create(cageElevator._platformBody.gameObject,
				cageElevator.gameObject.GetComponentInParent<Sector>(),
				x =>
				{
					x.gameObject.AddComponent<OWTriggerVolume>();
					var shape = x.gameObject.AddComponent<BoxShape>();
					shape.size = new Vector3(2.5f, 4.25f, 2.5f);
					shape.center = new Vector3(0, 2.15f, 0);

					// When the cage elevator warps when entering/exiting the underground,
					// the player's sector detector is removed from the fake sector.
					// So when the elevator is moving and they leave the sector, it means they have warped
					// and should be added back in.
					x.OnOccupantExitSector.AddListener((e) => 
					{ 
						if (cageElevator.isMoving) x.AddOccupant(e); 
					});
				});
		}

		// prisoner elevator
		{
			var prisonerElevator = QSBWorldSync.GetUnityObject<PrisonCellElevator>();
			FakeSector.Create(prisonerElevator._elevatorBody.gameObject,
				prisonerElevator.gameObject.GetComponentInParent<Sector>(),
				x =>
				{
					x.gameObject.AddComponent<OWTriggerVolume>();
					var shape = x.gameObject.AddComponent<BoxShape>();
					shape.size = new Vector3(4f, 6.75f, 6.7f);
					shape.center = new Vector3(0, 3.3f, 3.2f);
				});
		}


		//black hole forge
		{
			var forge = GameObject.Find("BlackHoleForgePivot");
			FakeSector.Create(forge,
				forge.GetComponentInParent<Sector>(),
				x =>
				{
					var trigger = x.gameObject.AddComponent<OWTriggerVolume>();
					x._triggerRoot = GameObject.Find("BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/" +
						"Sector_HangingCity/Sector_HangingCity_BlackHoleForge/BlackHoleForgePivot/" +
						"Volumes_BlackHoleForge/DirectionalForceVolume");
				});
		}

		// black hole forge entrance elevator
		{
			var entrance = GameObject.Find("BlackHoleForge_EntrancePivot");
			var sector = GameObject.Find("Sector_HangingCity_BlackHoleForge").GetComponent<Sector>();
			FakeSector.Create(entrance,
				sector,
				x =>
				{
					x.gameObject.AddComponent<OWTriggerVolume>();
					var shape = x.gameObject.AddComponent<BoxShape>();
					shape.size = new Vector3(5.5f, 5.8f, 5.5f);
					shape.center = new Vector3(0f, 2.9f, 1.5f);
				});
		}

		// OPC probe
		{
			var probe = Locator._orbitalProbeCannon
				.GetRequiredComponent<OrbitalProbeLaunchController>()
				._probeBody;
			if (probe)
			{
				// just create a big circle around the probe lol
				FakeSector.Create(probe.gameObject,
					null,
					x =>
					{
						x.gameObject.AddComponent<OWTriggerVolume>();
						x.gameObject.AddComponent<SphereShape>().radius = 100;
					});
			}
		}
	}
}
