using OWML.Common;
using QSB.Events;
using QSB.Player;
using QSB.QuantumSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace QSB.QuantumSync
{
	internal class QuantumManager : MonoBehaviour
	{
		public static QuantumManager Instance { get; private set; }
		public QuantumShrine Shrine;
		public bool IsReady;

		public void Awake()
		{
			Instance = this;
			QSBSceneManager.OnUniverseSceneLoaded += RebuildQuantumObjects;
		}

		public void OnDestroy() => QSBSceneManager.OnUniverseSceneLoaded -= RebuildQuantumObjects;

		public void RebuildQuantumObjects(OWScene scene)
		{
			DebugLog.DebugWrite("Rebuilding quantum objects...", MessageType.Warning);
			QSBWorldSync.Init<QSBSocketedQuantumObject, SocketedQuantumObject>();
			QSBWorldSync.Init<QSBMultiStateQuantumObject, MultiStateQuantumObject>();
			QSBWorldSync.Init<QSBQuantumSocket, QuantumSocket>();
			QSBWorldSync.Init<QSBQuantumShuffleObject, QuantumShuffleObject>();
			if (scene == OWScene.SolarSystem)
			{
				Shrine = Resources.FindObjectsOfTypeAll<QuantumShrine>().First();
			}
			IsReady = true;
		}

		public void CheckExistingPlayers()
		{
			DebugLog.DebugWrite("Checking quantum objects for non-existent players...", MessageType.Info);
			var quantumObjects = QSBWorldSync.GetWorldObjects<IQSBQuantumObject>().ToList();
			for (var i = 0; i < quantumObjects.Count; i++)
			{
				var obj = quantumObjects[i];
				if (!QSBPlayerManager.PlayerExists(obj.ControllingPlayer))
				{
					var idToSend = obj.IsEnabled ? QSBPlayerManager.LocalPlayerId : 0u;
					QSBEventManager.FireEvent(EventNames.QSBQuantumAuthority, i, idToSend);
				}
			}
		}

		public void OnRenderObject()
		{
			if (!QSBCore.HasWokenUp || !QSBCore.DebugMode || !QSBCore.ShowLinesInDebug)
			{
				return;
			}

			if (Shrine != null)
			{
				Popcron.Gizmos.Sphere(Shrine.transform.position, 10f, Color.magenta);
			}
		}

		public static bool IsVisibleUsingCameraFrustum(ShapeVisibilityTracker tracker, bool ignoreLocalCamera)
		{
			return tracker.gameObject.activeInHierarchy
				&& QSBPlayerManager.GetPlayerCameras(!ignoreLocalCamera)
					.Any(x => (bool)tracker.GetType()
						.GetMethod("IsInFrustum", BindingFlags.NonPublic | BindingFlags.Instance)
						.Invoke(tracker, new object[] { x.GetFrustumPlanes() }));
		}

		public static bool IsVisible(ShapeVisibilityTracker tracker, bool ignoreLocalCamera)
		{
			return tracker.gameObject.activeInHierarchy
				&& IsVisibleUsingCameraFrustum(tracker, ignoreLocalCamera)
				&& QSBPlayerManager.GetPlayerCameras(!ignoreLocalCamera)
					.Any(x => VisibilityOccluder.CanYouSee(tracker, x.mainCamera.transform.position));
		}

		public static IEnumerable<PlayerInfo> GetEntangledPlayers(QuantumObject obj)
		{
			IQSBQuantumObject worldObj = null;
			if (obj.GetType() == typeof(SocketedQuantumObject))
			{
				worldObj = QSBWorldSync.GetWorldObject<QSBSocketedQuantumObject, SocketedQuantumObject>((SocketedQuantumObject)obj);
			}
			else if (obj.GetType() == typeof(MultiStateQuantumObject))
			{
				worldObj = QSBWorldSync.GetWorldObject<QSBMultiStateQuantumObject, MultiStateQuantumObject>((MultiStateQuantumObject)obj);
			}
			else
			{
				DebugLog.ToConsole($"Warning - Get players entangled to Shuffle object?!?! Object:{obj.name}", MessageType.Warning);
			}
			return QSBPlayerManager.PlayerList.Where(x => x.EntangledObject == worldObj);
		}

		public static int GetId(IQSBQuantumObject obj)
			=> QSBWorldSync
				.GetWorldObjects<IQSBQuantumObject>()
				.ToList()
				.IndexOf(obj);

		public static IQSBQuantumObject GetObject(int id)
			=> QSBWorldSync
				.GetWorldObjects<IQSBQuantumObject>()
				.ToList()[id];
	}
}