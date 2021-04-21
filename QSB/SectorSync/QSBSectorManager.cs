using OWML.Common;
using QSB.SectorSync.WorldObjects;
using QSB.TransformSync;
using QSB.Utility;
using QSB.WorldSync;
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

		public void Invoke()
		{
			foreach (var sync in Resources.FindObjectsOfTypeAll<QSBNetworkTransform>()) // todo : cache these!
			{
				if (sync.AttachedObject == null)
				{
					continue;
				}
				if (sync.HasAuthority && sync.AttachedObject.activeInHierarchy)
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

		private void CheckTransformSyncSector(QSBNetworkTransform transformSync)
		{
			var syncedTransform = transformSync.AttachedObject;
			if (syncedTransform == null || syncedTransform.transform.position == Vector3.zero)
			{
				return;
			}
			var closestSector = transformSync.SectorSync.GetClosestSector(syncedTransform.transform);
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