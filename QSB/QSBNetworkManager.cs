using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB {
    class QSBNetworkManager: NetworkManager {
        void Awake () {
            var assetBundle = QSB.Helper.Assets.LoadBundle("assets/network");
            playerPrefab = assetBundle.LoadAsset<GameObject>("assets/networkplayer.prefab");
            playerPrefab.AddComponent<NetworkPlayer>();

            try
            {
                var anim = playerPrefab.GetComponentsInChildren<Animator>()
                    .Single(x => x.name == "Traveller_HEA_Player_v2");
                var netAnim = playerPrefab.AddComponent<NetworkAnimator>();
                netAnim.animator = anim;
                for (var i = 0; i < 20; i++)
                {
                    try
                    {
                        if (anim.GetParameter(i) != null)
                        {
                            netAnim.SetParameterAutoSend(i, true);
                        }
                        else
                        {
                            DebugLog.Console("Parameter", i, "is null");
                        }
                    }
                    catch (Exception ex)
                    {
                        DebugLog.Console("Error while getting parameter", i, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLog.Console("Error setting up anim sync", ex);
            }
        }

        public override void OnStartServer () {
            WakeUpSync.isServer = true;
        }

        public override void OnClientConnect (NetworkConnection conn) {
            base.OnClientConnect(conn);

            DebugLog.Screen("OnClientConnect");
            gameObject.AddComponent<WakeUpSync>();
            gameObject.AddComponent<SectorSync>();
        }
    }
}
