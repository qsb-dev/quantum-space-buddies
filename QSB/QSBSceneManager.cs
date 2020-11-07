using OWML.Common;
using QSB.Utility;
using System;

namespace QSB
{
    public static class QSBSceneManager
    {
        public static OWScene CurrentScene => LoadManager.GetCurrentScene();

        public static bool IsInUniverse => InUniverse(CurrentScene);

        public static event Action<OWScene, bool> OnSceneLoaded;

        static QSBSceneManager()
        {
            LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;
            DebugLog.DebugWrite("Scene Manager ready.", MessageType.Success);
        }

        private static void OnCompleteSceneLoad(OWScene oldScene, OWScene newScene)
        {
            OnSceneLoaded?.Invoke(newScene, InUniverse(newScene));
        }

        private static bool InUniverse(OWScene scene)
        {
            return scene == OWScene.SolarSystem || scene == OWScene.EyeOfTheUniverse;
        }
    }
}
