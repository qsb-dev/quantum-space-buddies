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

        private void Awake()
        {
            Application.runInBackground = true;
        }

        private void Start()
        {
            DebugLog.ToConsole($"* Start of QSB version {ModHelper.Manifest.Version} - authored by {ModHelper.Manifest.Author}");

            Helper = ModHelper;
            NetworkAssetBundle = Helper.Assets.LoadBundle("assets/network");
            Patches.DoPatches();

            gameObject.AddComponent<DebugActions>();
            gameObject.AddComponent<ElevatorManager>();
            gameObject.AddComponent<GeyserManager>();
            gameObject.AddComponent<NetworkManagerHUD>();
            gameObject.AddComponent<QSBNetworkManager>();
            gameObject.AddComponent<QSBSectorManager>();
        }

        public override void Configure(IModConfig config)
        {
            DefaultServerIP = config.GetSettingsValue<string>("defaultServerIP");
            DebugMode = config.GetSettingsValue<bool>("debugMode");
        }
    }
}
