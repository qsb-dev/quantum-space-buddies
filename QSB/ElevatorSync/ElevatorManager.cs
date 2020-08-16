using System.Collections.Generic;
using System.Linq;
using QSB.WorldSync;
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
                var qsbElevator = new QSBElevator();
                qsbElevator.Init(_elevators[id], id);
                WorldRegistry.AddObject(qsbElevator);
            }
        }

        public int GetId(Elevator elevator) => _elevators.IndexOf(elevator);
    }
}
