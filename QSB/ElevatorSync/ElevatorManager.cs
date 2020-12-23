using QSB.WorldSync;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.ElevatorSync
{
	public class ElevatorManager : MonoBehaviour
	{
		public static ElevatorManager Instance { get; private set; }

		private List<Elevator> _elevators;

		public void Awake()
		{
			Instance = this;
			QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
		}

		public void OnDestroy() => QSBSceneManager.OnSceneLoaded -= OnSceneLoaded;

		private void OnSceneLoaded(OWScene scene, bool isInUniverse) => QSBWorldSync.Init<QSBElevator, Elevator>(ref _elevators);

		public int GetId(Elevator elevator) => _elevators.IndexOf(elevator);
	}
}