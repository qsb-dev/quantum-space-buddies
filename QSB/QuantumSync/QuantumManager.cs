using OWML.Utils;
using QSB.QuantumSync.WorldObjects;
using QSB.WorldSync;
using System.Collections.Generic;
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

		public void Awake()
		{
			Instance = this;
			QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
		}

		public void OnDestroy() => QSBSceneManager.OnSceneLoaded -= OnSceneLoaded;

		private void OnSceneLoaded(OWScene scene, bool isInUniverse)
		{
			_socketedQuantumObjects = QSBWorldSync.Init<QSBSocketedQuantumObject, SocketedQuantumObject>();
			_multiStateQuantumObjects = QSBWorldSync.Init<QSBMultiStateQuantumObject, MultiStateQuantumObject>();
			_quantumSockets = QSBWorldSync.Init<QSBQuantumSocket, QuantumSocket>();
			_quantumShuffleObjects = QSBWorldSync.Init<QSBQuantumShuffleObject, QuantumShuffleObject>();
		}

		private void OnRenderObject()
		{
			if (!QSBCore.HasWokenUp)
			{
				return;
			}
			foreach (var item in _socketedQuantumObjects)
			{
				if (!item.gameObject.activeInHierarchy)
				{
					continue;
				}
				Popcron.Gizmos.Sphere(item.transform.position, 5f, item.IsVisible() ? Color.green : Color.red);
			}
			Popcron.Gizmos.Sphere(Locator.GetQuantumMoon().transform.position, 120f, Color.cyan, true);
			var visTracker = Locator.GetQuantumMoon().GetValue<VisibilityTracker>("_visibilityTracker");
			Popcron.Gizmos.Sphere(visTracker.transform.position, 130f, visTracker.IsVisibleUsingCameraFrustum() ? Color.green : Color.red);
		}

		public int GetId(SocketedQuantumObject obj) => _socketedQuantumObjects.IndexOf(obj);
		public int GetId(MultiStateQuantumObject obj) => _multiStateQuantumObjects.IndexOf(obj);
		public int GetId(QuantumSocket obj) => _quantumSockets.IndexOf(obj);
		public int GetId(QuantumShuffleObject obj) => _quantumShuffleObjects.IndexOf(obj);
	}
}