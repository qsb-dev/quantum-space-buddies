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
				worldObj = QSBWorldSync.GetWorldObject<QSBSocketedQuantumObject, SocketedQuantumObject>((SocketedQuantumObject)unityObject);
			}
			else if (unityObject.GetType() == typeof(MultiStateQuantumObject))
			{
				worldObj = QSBWorldSync.GetWorldObject<QSBMultiStateQuantumObject, MultiStateQuantumObject>((MultiStateQuantumObject)unityObject);
			}
			else if (unityObject.GetType() == typeof(QuantumShuffleObject))
			{
				worldObj = QSBWorldSync.GetWorldObject<QSBQuantumShuffleObject, QuantumShuffleObject>((QuantumShuffleObject)unityObject);
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

		public static IQSBQuantumObject GetObject(int id)
		{
			var objects = QSBWorldSync
				.GetWorldObjects<IQSBQuantumObject>()
				.ToList();
			if (objects.Count == 0)
			{
				DebugLog.ToConsole($"Error - tried to get IQSBQuantumObject, but there are none!", MessageType.Error);
				return null;
			}
			if (objects.Count <= id)
			{
				DebugLog.ToConsole($"Error - Index {id} does not exist in list of IQSBObjects! (Count:{objects.Count})", MessageType.Error);
				return null;
			}
			if (id < 0)
			{
				DebugLog.ToConsole($"Error - tried to get IQSBQuantumObject with index less than zero...", MessageType.Error);
				return null;
			}
			return objects[id];
		}

		public static int GetId(IQSBQuantumObject obj)
		{
			var objects = QSBWorldSync
				.GetWorldObjects<IQSBQuantumObject>()
				.ToList();
			if (obj == null)
			{
				DebugLog.ToConsole($"Error - tried to get id of null IQSBQuantumObject!", MessageType.Error);
				return -1;
			}
			if (objects.Count == 0)
			{
				DebugLog.ToConsole($"Error - tried to get id of IQSBQuantumObject, but there are none!", MessageType.Error);
				return -1;
			}
			if (!objects.Contains(obj))
			{
				DebugLog.ToConsole($"Error - tried to get id of IQSBQuantumObject that doesn't exist in WorldObject list?!", MessageType.Error);
				return -1;
			}
			return objects.IndexOf(obj);
		}
	}
}