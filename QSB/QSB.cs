using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB
{
    public class QSB : ModBehaviour
    {
        public static IModHelper Helper;

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
            gameObject.AddComponent<PreserveTimeScale>();
        }
        
    }
}
