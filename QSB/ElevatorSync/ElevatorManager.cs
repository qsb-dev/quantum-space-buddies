using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.ElevatorSync
{
    public class ElevatorManager : MonoBehaviour
    {
        public static ElevatorManager Instance { get; private set; }

        private List<Elevator> _elevators;

        private void Awake()
        {
            Instance = this;

            LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;

            QSB.Helper.HarmonyHelper.AddPostfix<Elevator>("StartLift", typeof(ElevatorPatches), nameof(ElevatorPatches.StartLift));
        }

        private void OnCompleteSceneLoad(OWScene oldScene, OWScene newScene)
        {
            _elevators = Resources.FindObjectsOfTypeAll<Elevator>().ToList();
            for (var id = 0; id < _elevators.Count; id++)
            {
                var elevatorController = new QSBElevator();
                elevatorController.Init(_elevators[id], id);
            }
        }

        public int GetId(Elevator elevator) => _elevators.IndexOf(elevator);
    }
}
