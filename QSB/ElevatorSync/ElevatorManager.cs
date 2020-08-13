using UnityEngine;

namespace QSB.ElevatorSync
{
    public class ElevatorManager : MonoBehaviour
    {
        private void Awake()
        {
            QSB.Helper.Events.Subscribe<Elevator>(OWML.Common.Events.AfterAwake);
            QSB.Helper.Events.Event += OnEvent;

            QSB.Helper.HarmonyHelper.AddPostfix<Elevator>("StartLift", typeof(ElevatorPatches), nameof(ElevatorPatches.StartLift));
        }

        private void OnEvent(MonoBehaviour behaviour, OWML.Common.Events ev)
        {
            if (behaviour is Elevator elevator && ev == OWML.Common.Events.AfterAwake)
            {
                var elevatorController = gameObject.AddComponent<ElevatorController>();
                elevatorController.Init(elevator);
            }
        }
    }
}
