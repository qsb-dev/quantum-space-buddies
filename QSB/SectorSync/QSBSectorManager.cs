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

		// todo cage elevators
		// todo prisoner elevator
		// todo black hole forge

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
