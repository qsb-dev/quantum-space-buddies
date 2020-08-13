using OWML.Common;
using OWML.ModHelper;
using QSB.ElevatorSync;
using QSB.Events;
using QSB.GeyserSync;
using QSB.Utility;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace QSB
{
    public class QSB : ModBehaviour
    {
        public static IModHelper Helper;
        public static string DefaultServerIP;
        public static bool DebugMode;
        public static bool WokenUp;

        private void Awake()
        {
            Application.runInBackground = true;
        }

        private void Start()
        {
            Helper = ModHelper;

            gameObject.AddComponent<DebugLog>();
            gameObject.AddComponent<QSBNetworkManager>();
            gameObject.AddComponent<NetworkManagerHUD>();
            gameObject.AddComponent<DebugActions>();
            gameObject.AddComponent<UnityHelper>();
            gameObject.AddComponent<ElevatorManager>();

            GlobalMessenger.AddListener(EventNames.RestartTimeLoop, OnLoopStart);
            GlobalMessenger.AddListener(EventNames.WakeUp, OnWakeUp);

            LoadManager.OnCompleteSceneLoad += OnSceneLoad;
        }

        private void OnWakeUp()
        {
            WokenUp = true;
            GlobalMessenger.FireEvent(EventNames.QSBPlayerStatesRequest);
        }

        private void OnLoopStart()
        {
            WokenUp = false;
        }

        private void OnSceneLoad(OWScene oldScene, OWScene newScene)
        {
            WorldRegistry.GenerateComponentList();
        }

        public override void Configure(IModConfig config)
        {
            DefaultServerIP = config.GetSettingsValue<string>("defaultServerIP");
            DebugMode = config.GetSettingsValue<bool>("debugMode");
        }
    }
}
