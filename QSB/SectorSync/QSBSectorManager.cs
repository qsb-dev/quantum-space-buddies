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
			if (sync.hasAuthority
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
		await this.Try("creating fake sectors", () => CreateFakeSectors(ct));

		QSBWorldSync.Init<QSBSector, Sector>();
		_isReady = QSBWorldSync.GetWorldObjects<QSBSector>().Any();
	}

	public override void UnbuildWorldObjects() =>
		_isReady = false;

	private static async UniTask CreateFakeSectors(CancellationToken ct)
	{
		if (QSBSceneManager.CurrentScene != OWScene.SolarSystem)
		{
			return;
		}

		// time loop spinning ring
		{
			var TimeLoopRing_Body = GameObject.Find("TimeLoopRing_Body");
			var Sector_TimeLoopInterior = GameObject.Find("Sector_TimeLoopInterior").GetComponent<Sector>();
			// use same shape as parent sector
			var shape = (SphereShape)Sector_TimeLoopInterior.GetTriggerVolume().GetShape();
			FakeSector.Create<SphereShape>(TimeLoopRing_Body, Sector_TimeLoopInterior,
				x => x.radius = shape.radius);
		}

		// TH elevators
		foreach (var elevator in QSBWorldSync.GetUnityObjects<Elevator>())
		{
			// hack: wait for QSBElevator to add the box shape, and just use that
			BoxShape shape = null;
			await UniTask.WaitUntil(() => elevator.TryGetComponent(out shape), cancellationToken: ct);

			FakeSector.Create<BoxShape>(elevator.gameObject,
				elevator.GetComponentInParent<Sector>(),
				x =>
				{
					x.center = shape.center;
					x.size = shape.size;
				});
		}

		// rafts
		foreach (var raft in QSBWorldSync.GetUnityObjects<RaftController>())
		{
			FakeSector.Create<BoxShape>(raft.gameObject,
				raft._sector,
				x =>
				{
					// todo: figure out a good shape for the raft ride volume
					x.size = Vector3.one * 10;
				});
		}

		// todo cage elevators
		// todo prisoner elevator

		// OPC probe
		{
			var probe = Locator._orbitalProbeCannon
				.GetRequiredComponent<OrbitalProbeLaunchController>()
				._probeBody;
			if (probe)
			{
				FakeSector.Create<SphereShape>(probe.gameObject,
					null,
					x =>
					{
						// todo: figure out a good radius for this sector
						x.radius = 100;
					});
			}
		}
	}
}
