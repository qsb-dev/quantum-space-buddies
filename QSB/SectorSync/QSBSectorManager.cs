using OWML.Common;
using QSB.SectorSync.WorldObjects;
using QSB.Syncs;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.SectorSync
{
	public class QSBSectorManager : WorldObjectManager, IRepeating
	{
		public static QSBSectorManager Instance { get; private set; }
		public bool IsReady { get; private set; }
		public List<QSBSector> FakeSectors = new List<QSBSector>();

		private void OnEnable() => RepeatingManager.Repeatings.Add(this);
		private void OnDisable() => RepeatingManager.Repeatings.Remove(this);

		public List<ISectoredSync<Transform>> SectoredTransformSyncs = new List<ISectoredSync<Transform>>();
		public List<ISectoredSync<OWRigidbody>> SectoredRigidbodySyncs = new List<ISectoredSync<OWRigidbody>>();

		public void Invoke()
		{
			foreach (var sync in SectoredTransformSyncs)
			{
				if (sync.AttachedObject == null)
				{
					continue;
				}

				if ((sync as QNetworkBehaviour).HasAuthority
					&& sync.AttachedObject.gameObject.activeInHierarchy
					&& sync.IsReady)
				{
					CheckTransformSyncSector(sync);
				}
			}

			foreach (var sync in SectoredRigidbodySyncs)
			{
				if (sync.AttachedObject == null)
				{
					continue;
				}

				if ((sync as QNetworkBehaviour).HasAuthority
					&& sync.AttachedObject.gameObject.activeInHierarchy
					&& sync.IsReady)
				{
					CheckTransformSyncSector(sync);
				}
			}
		}

		public override void Awake()
		{
			base.Awake();
			Instance = this;
			DebugLog.DebugWrite("Sector Manager ready.", MessageType.Success);
		}

		protected override void RebuildWorldObjects(OWScene scene)
		{
			DebugLog.DebugWrite("Rebuilding sectors...", MessageType.Warning);
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

		private void CheckTransformSyncSector<T>(ISectoredSync<T> transformSync)
			where T : Component
		{
			var attachedObject = transformSync.AttachedObject;
			var closestSector = transformSync.SectorSync.GetClosestSector(attachedObject.transform);
			if (closestSector == default(QSBSector))
			{
				return;
			}

			if (closestSector == transformSync.ReferenceSector)
			{
				return;
			}

			transformSync.SetReferenceSector(closestSector);
		}
	}
}