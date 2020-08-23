using QSB.WorldSync;
using UnityEngine;

namespace QSB.GeyserSync
{
    public class GeyserManager : MonoBehaviour
    {
        public static GeyserManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(OWScene scene, bool isInUniverse)
        {
            var geyserControllers = Resources.FindObjectsOfTypeAll<GeyserController>();
            for (var id = 0; id < geyserControllers.Length; id++)
            {
                var qsbGeyser = new QSBGeyser();
                qsbGeyser.Init(geyserControllers[id], id);
                WorldRegistry.AddObject(qsbGeyser);
            }
        }

        public void EmptyUpdate()
        {
            QSB.Helper.HarmonyHelper.EmptyMethod<GeyserController>("Update");
        }

    }
}