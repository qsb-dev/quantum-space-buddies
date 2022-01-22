using OWML.Common;
using QSB.SectorSync.WorldObjects;
using QSB.Syncs.Sectored;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.SectorSync
{
	public class QSBSectorManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.Both;

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

		public override void BuildWorldObjects(OWScene scene)
		{
			DebugLog.DebugWrite("Building sectors...", MessageType.Info);
			if (QSBSceneManager.CurrentScene == OWScene.SolarSystem)
			{
				var timeLoopRing = GameObject.Find("TimeLoopRing_Body");
				if (timeLoopRing != null)
				{
					if (timeLoopRing.GetComponent<FakeSector>() == null)
					{
						timeLoopRing.AddComponent<FakeSector>().AttachedSector = GameObject.Find("Sector_TimeLoopInterior").GetComponent<Sector>();
					}
				}
				else
				{
					DebugLog.ToConsole($"Error - TimeLoopRing_Body not found!", MessageType.Error);
				}
			}

			QSBWorldSync.Init<QSBSector, Sector>();
			_isReady = QSBWorldSync.GetWorldObjects<QSBSector>().Any();
		}

		public override void UnbuildWorldObjects() =>
			_isReady = false;
	}
}
