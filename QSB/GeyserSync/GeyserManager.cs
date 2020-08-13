using UnityEngine;

namespace QSB.GeyserSync
{
    public class GeyserManager : MonoBehaviour
    {
        public static GeyserManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;

            QSB.Helper.Events.Subscribe<GeyserController>(OWML.Common.Events.AfterAwake);
            QSB.Helper.Events.Event += OnEvent;
        }

        private void OnEvent(MonoBehaviour behaviour, OWML.Common.Events ev)
        {
            if (behaviour is GeyserController geyserController && ev == OWML.Common.Events.AfterAwake)
            {
                var geyser = new QSBGeyser();
                geyser.Init(geyserController);
            }
        }

        public void EmptyUpdate()
        {
            QSB.Helper.HarmonyHelper.EmptyMethod<GeyserController>("Update");
        }

    }
}