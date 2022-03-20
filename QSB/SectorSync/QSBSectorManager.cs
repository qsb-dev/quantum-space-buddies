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
	public readonly List<QSBSector> FakeSectors = new();

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
		await CreateFakeSectors(ct);

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

		var TimeLoopRing_Body = GameObject.Find("TimeLoopRing_Body");
		var Sector_TimeLoopInterior = GameObject.Find("Sector_TimeLoopInterior").GetComponent<Sector>();
		// use same radius as parent sector
		var radius = Sector_TimeLoopInterior.GetTriggerVolume().GetShape().CalcWorldBounds().radius;
		FakeSector.CreateOn(TimeLoopRing_Body, radius, Sector_TimeLoopInterior);

		foreach (var elevator in QSBWorldSync.GetUnityObjects<Elevator>())
		{
			radius = float.MinValue;
			foreach (var collider in elevator.GetComponentsInChildren<Collider>())
			{
				await UniTask.WaitUntil(() => collider.bounds.extents != Vector3.zero, cancellationToken: ct);
				radius = Mathf.Max(radius, collider.bounds.extents.magnitude);
			}

			FakeSector.CreateOn(elevator.gameObject,
				radius,
				elevator.GetComponentInParent<Sector>());
		}
	}
}
