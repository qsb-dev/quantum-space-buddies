using OWML.Common;
using OWML.ModHelper;
using QSB.ElevatorSync;
using QSB.GeyserSync;
using QSB.TransformSync;
using QSB.Utility;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB
{
    public class QSB : ModBehaviour
    {
        public static IModHelper Helper;
        public static string DefaultServerIP;
        public static bool DebugMode;
        public static AssetBundle NetworkAssetBundle;
        private static GameObject GameObject;

        private void Awake()
        {
            Application.runInBackground = true;
        }

        private void Start()
        {
            Helper = ModHelper;
            GameObject = gameObject;

            NetworkAssetBundle = Helper.Assets.LoadBundle("assets/network");
            QSB.Helper.HarmonyHelper.EmptyMethod<NetworkManagerHUD>("Update");

            gameObject.AddComponent<DebugLog>();
            gameObject.AddComponent<QSBNetworkManager>();
            gameObject.AddComponent<NetworkManagerHUD>();
            gameObject.AddComponent<DebugActions>();
            gameObject.AddComponent<UnityHelper>();
            gameObject.AddComponent<ElevatorManager>();
            gameObject.AddComponent<GeyserManager>();
            gameObject.AddComponent<QSBSectorManager>();
        }

        public override void Configure(IModConfig config)
        {
            DefaultServerIP = config.GetSettingsValue<string>("defaultServerIP");
            DebugMode = config.GetSettingsValue<bool>("debugMode");
        }
    }
}
