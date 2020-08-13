using UnityEngine;

namespace QSB.GeyserSync
{
    public class GeyserManager : MonoBehaviour
    {
        public static GeyserManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;

            LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;
        }

        private void OnCompleteSceneLoad(OWScene oldScene, OWScene newScene)
        {
            var geyserControllers = Resources.FindObjectsOfTypeAll<GeyserController>();
            for (var id = 0; id < geyserControllers.Length; id++)
            {
                var geyser = new QSBGeyser();
                geyser.Init(geyserControllers[id], id);
            }
        }
        
        public void EmptyUpdate()
        {
            QSB.Helper.HarmonyHelper.EmptyMethod<GeyserController>("Update");
        }

    }
}