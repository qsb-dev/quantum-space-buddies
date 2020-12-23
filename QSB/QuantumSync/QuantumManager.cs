using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.QuantumSync
{
	internal class QuantumManager : MonoBehaviour
	{
		public static QuantumManager Instance { get; private set; }

		private List<SocketedQuantumObject> _socketedQuantumObjects;
		private List<QuantumSocket> _quantumSockets;

		public void Awake()
		{
			Instance = this;
			QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
		}

		private void OnSceneLoaded(OWScene scene, bool isInUniverse)
		{
			_socketedQuantumObjects = Resources.FindObjectsOfTypeAll<SocketedQuantumObject>().ToList();
			for (var id = 0; id < _socketedQuantumObjects.Count; id++)
			{
				var qsbSocketQuantumObj = QSBWorldSync.GetWorldObject<QSBSocketedQuantumObject>(id) ?? new QSBSocketedQuantumObject();
				qsbSocketQuantumObj.Init(_socketedQuantumObjects[id], id);
				QSBWorldSync.AddWorldObject(qsbSocketQuantumObj);
			}

			_quantumSockets = Resources.FindObjectsOfTypeAll<QuantumSocket>().ToList();
			for (var id = 0; id < _quantumSockets.Count; id++)
			{
				var qsbQuantumSocket = QSBWorldSync.GetWorldObject<QSBQuantumSocket>(id) ?? new QSBQuantumSocket();
				qsbQuantumSocket.Init(_quantumSockets[id], id);
				QSBWorldSync.AddWorldObject(qsbQuantumSocket);
			}
		}

		public int GetId(SocketedQuantumObject obj) => _socketedQuantumObjects.IndexOf(obj);
		public int GetId(QuantumSocket obj) => _quantumSockets.IndexOf(obj);
	}
}