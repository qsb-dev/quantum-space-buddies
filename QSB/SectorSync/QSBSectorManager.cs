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
		public bool IsReady { get; private set; }
		public readonly List<QSBSector> FakeSectors = new();

		public readonly List<BaseSectoredSync> TransformSyncs = new();

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
			if (!Instance.IsReady || !QSBWorldSync.AllObjectsReady)
			{
				return;
			}

			foreach (var sync in TransformSyncs)
			{
				if (sync.AttachedTransform == null)
				{
					continue;
				}

				if (sync.hasAuthority
					&& sync.AttachedTransform.gameObject.activeInHierarchy
					&& sync.IsInitialized
					&& sync.SectorSync.IsReady)
				{
					UpdateReferenceSector(sync);
				}
			}
		}

		public void Awake()
		{
			Instance = this;
			DebugLog.DebugWrite("Sector Manager ready.", MessageType.Success);
		}

		public override void RebuildWorldObjects(OWScene scene)
		{
			DebugLog.DebugWrite("Rebuilding sectors...", MessageType.Info);
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
			IsReady = QSBWorldSync.GetWorldObjects<QSBSector>().Any();
		}

		private static void UpdateReferenceSector(BaseSectoredSync transformSync)
		{
			var closestSector = transformSync.SectorSync.GetClosestSector();
			if (closestSector == null)
			{
				return;
			}

			transformSync.SetReferenceSector(closestSector);
		}
	}
}
