using QSB.ElevatorSync.WorldObjects;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.ElevatorSync
{
	public class ElevatorManager : MonoBehaviour
	{
		public static ElevatorManager Instance { get; private set; }

		public void Awake()
		{
			Instance = this;
			QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
		}

		public void OnDestroy() => QSBSceneManager.OnSceneLoaded -= OnSceneLoaded;

		private void OnSceneLoaded(OWScene scene, bool isInUniverse)
			=> QSBWorldSync.Init<QSBElevator, Elevator>();
	}
}