using OWML.Common;
using OWML.Utils;
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
			if (!QSBCore.HasWokenUp || !QSBCore.DebugMode)
			{
				return;
			}

			if (QSBSceneManager.CurrentScene != OWScene.SolarSystem)
			{
				return;
			}

			GUI.Label(new Rect(220, 10, 200f, 20f), $"QM Visible : {Locator.GetQuantumMoon().IsVisible()}");
			var offset = 40f;
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
			GUI.Label(new Rect(440, offset, 200f, 20f), $"Players in QM :");
			offset += 30f;
			foreach (var player in QSBPlayerManager.PlayerList.Where(x => x.IsInMoon))
			{
				GUI.Label(new Rect(440, offset, 200f, 20f), $"- {player.PlayerId}");
				offset += 30f;
			}
			GUI.Label(new Rect(440, offset, 200f, 20f), $"Players in Shrine :");
			offset += 30f;
			foreach (var player in QSBPlayerManager.PlayerList.Where(x => x.IsInShrine))
			{
				GUI.Label(new Rect(440, offset, 200f, 20f), $"- {player.PlayerId}");
				offset += 30f;
			}
		}

		public static bool IsVisibleUsingCameraFrustum(ShapeVisibilityTracker tracker, bool skipVisibilityCheck)
		{
			return tracker.gameObject.activeInHierarchy
				&& QSBPlayerManager.GetPlayerCameras(!skipVisibilityCheck)
					.Any(x => (bool)tracker.GetType()
						.GetMethod("IsInFrustum", BindingFlags.NonPublic | BindingFlags.Instance)
						.Invoke(tracker, new object[] { x.GetFrustumPlanes() }));
		}

		public static bool IsVisible(ShapeVisibilityTracker tracker, bool skipVisibilityCheck)
		{
			return tracker.gameObject.activeInHierarchy
				&& IsVisibleUsingCameraFrustum(tracker, skipVisibilityCheck)
				&& QSBPlayerManager.GetPlayerCameras(!skipVisibilityCheck)
					.Any(x => VisibilityOccluder.CanYouSee(tracker, x.mainCamera.transform.position));
		}

		public int GetId(SocketedQuantumObject obj) => _socketedQuantumObjects.IndexOf(obj);
		public int GetId(MultiStateQuantumObject obj) => _multiStateQuantumObjects.IndexOf(obj);
		public int GetId(QuantumSocket obj) => _quantumSockets.IndexOf(obj);
		public int GetId(QuantumShuffleObject obj) => _quantumShuffleObjects.IndexOf(obj);
	}
}