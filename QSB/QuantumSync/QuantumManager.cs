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
			QSBWorldSync.Init<QSBQuantumMoon, QuantumMoon>();
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
				&& QSBPlayerManager.GetPlayersWithCameras(!ignoreLocalCamera)
					.Any(x => (bool)tracker.GetType()
						.GetMethod("IsInFrustum", BindingFlags.NonPublic | BindingFlags.Instance)
						.Invoke(tracker, new object[] { x.Camera.GetFrustumPlanes() }));
		}

		public static bool IsVisible(ShapeVisibilityTracker tracker, bool ignoreLocalCamera)
		{
			return tracker.gameObject.activeInHierarchy
				&& IsVisibleUsingCameraFrustum(tracker, ignoreLocalCamera)
				&& QSBPlayerManager.GetPlayersWithCameras(!ignoreLocalCamera)
					.Any(x => VisibilityOccluder.CanYouSee(tracker, x.Camera.mainCamera.transform.position));
		}

		public static IEnumerable<PlayerInfo> GetEntangledPlayers(QuantumObject obj)
		{
			var worldObj = GetObject(obj);
			return QSBPlayerManager.PlayerList.Where(x => x.EntangledObject == worldObj);
		}

		public static IQSBQuantumObject GetObject(QuantumObject unityObject)
		{
			IQSBQuantumObject worldObj = null;
			if (unityObject.GetType() == typeof(SocketedQuantumObject) || unityObject.GetType() == typeof(QuantumShrine))
			{
				worldObj = QSBWorldSync.GetWorldFromUnity<QSBSocketedQuantumObject, SocketedQuantumObject>((SocketedQuantumObject)unityObject);
			}
			else if (unityObject.GetType() == typeof(MultiStateQuantumObject))
			{
				worldObj = QSBWorldSync.GetWorldFromUnity<QSBMultiStateQuantumObject, MultiStateQuantumObject>((MultiStateQuantumObject)unityObject);
			}
			else if (unityObject.GetType() == typeof(QuantumShuffleObject))
			{
				worldObj = QSBWorldSync.GetWorldFromUnity<QSBQuantumShuffleObject, QuantumShuffleObject>((QuantumShuffleObject)unityObject);
			}
			else if (unityObject.GetType() == typeof(QuantumMoon))
			{
				worldObj = QSBWorldSync.GetWorldObject<QSBQuantumMoon, QuantumMoon>((QuantumMoon)unityObject);
			}
			else
			{
				DebugLog.ToConsole($"Warning - couldn't work out type of QuantumObject {unityObject.name}.", MessageType.Warning);
			}
			return worldObj;
		}
	}
}