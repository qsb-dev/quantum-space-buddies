using OWML.Common;
using OWML.Utils;
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

		private List<SocketedQuantumObject> _socketedQuantumObjects;
		private List<MultiStateQuantumObject> _multiStateQuantumObjects;
		private List<QuantumSocket> _quantumSockets;
		private List<QuantumShuffleObject> _quantumShuffleObjects;
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
			_socketedQuantumObjects = QSBWorldSync.Init<QSBSocketedQuantumObject, SocketedQuantumObject>();
			_multiStateQuantumObjects = QSBWorldSync.Init<QSBMultiStateQuantumObject, MultiStateQuantumObject>();
			_quantumSockets = QSBWorldSync.Init<QSBQuantumSocket, QuantumSocket>();
			_quantumShuffleObjects = QSBWorldSync.Init<QSBQuantumShuffleObject, QuantumShuffleObject>();
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

		public void OnGUI()
		{
			GUI.Label(new Rect(220, 10, 200f, 20f), $"HasWokenUp : {QSBCore.HasWokenUp}");

			if (!QSBCore.HasWokenUp || !QSBCore.DebugMode)
			{
				return;
			}

			if (QSBSceneManager.CurrentScene != OWScene.SolarSystem)
			{
				return;
			}

			var offset = 40f;
			GUI.Label(new Rect(220, offset, 200f, 20f), $"QM Visible : {Locator.GetQuantumMoon().IsVisible()}");
			offset += 30f;
			GUI.Label(new Rect(220, offset, 200f, 20f), $"QM Locked : {Locator.GetQuantumMoon().IsLocked()}");
			offset += 30f;
			GUI.Label(new Rect(220, offset, 200f, 20f), $"QM Illuminated : {Locator.GetQuantumMoon().IsIlluminated()}");
			offset += 30f;
			GUI.Label(new Rect(220, offset, 200f, 20f), $"Shrine player dark? : {Shrine.IsPlayerInDarkness()}");
			offset += 30f;
			GUI.Label(new Rect(220, offset, 200f, 20f), $"Shrine player inside? : {Shrine.IsPlayerInside()}");
			offset += 30f;
			var tracker = Locator.GetQuantumMoon().GetValue<ShapeVisibilityTracker>("_visibilityTracker");
			foreach (var camera in QSBPlayerManager.GetPlayerCameras())
			{
				GUI.Label(new Rect(220, offset, 200f, 20f), $"- {camera.name} : {tracker.GetType().GetMethod("IsInFrustum", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(tracker, new object[] { camera.GetFrustumPlanes() })}");
				offset += 30f;
			}

			// Used for diagnosing specific socketed objects. Just set <index> to be the correct index.
			/*
			var index = 110;
			var socketedObject = QSBWorldSync.GetWorldObject<QSBSocketedQuantumObject>(index);
			GUI.Label(new Rect(220, offset, 200f, 20f), $"{index} Controller : {socketedObject.ControllingPlayer}");
			offset += 30f;
			GUI.Label(new Rect(220, offset, 200f, 20f), $"{index} Visible : {socketedObject.AttachedObject.IsVisible()}");
			offset += 30f;
			GUI.Label(new Rect(220, offset, 200f, 20f), $"{index} Locked : {socketedObject.AttachedObject.IsLocked()}");
			offset += 30f;
			GUI.Label(new Rect(220, offset, 200f, 20f), $"{index} Illuminated : {socketedObject.AttachedObject.IsIlluminated()}");
			offset += 30f;
			var socketedTrackers = socketedObject.AttachedObject.GetComponentsInChildren<ShapeVisibilityTracker>();
			if (socketedTrackers == null || socketedTrackers.Length == 0)
			{
				GUI.Label(new Rect(220, offset, 200f, 20f), $"- List is null or empty.");
				return;
			}
			if (socketedTrackers.Any(x => x is null))
			{
				GUI.Label(new Rect(220, offset, 200f, 20f), $"- Uses a null.");
				return;
			}
			foreach (var camera in QSBPlayerManager.GetPlayerCameras())
			{
				GUI.Label(new Rect(220, offset, 200f, 20f), $"- {camera.name} : {socketedTrackers.Any(x => (bool)x.GetType().GetMethod("IsInFrustum", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(x, new object[] { camera.GetFrustumPlanes() }))}");
				offset += 30f;
			}
			*/

			offset = 10f;
			GUI.Label(new Rect(440, offset, 200f, 20f), $"Owned Objects :");
			offset += 30f;
			foreach (var obj in QSBWorldSync.GetWorldObjects<IQSBQuantumObject>().Where(x => x.ControllingPlayer == QSBPlayerManager.LocalPlayerId))
			{
				GUI.Label(new Rect(440, offset, 200f, 20f), $"- {(obj as IWorldObject).Name}");
				offset += 30f;
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

		public int GetId(SocketedQuantumObject obj) => _socketedQuantumObjects.IndexOf(obj);
		public int GetId(MultiStateQuantumObject obj) => _multiStateQuantumObjects.IndexOf(obj);
		public int GetId(QuantumSocket obj) => _quantumSockets.IndexOf(obj);
		public int GetId(QuantumShuffleObject obj) => _quantumShuffleObjects.IndexOf(obj);
	}
}