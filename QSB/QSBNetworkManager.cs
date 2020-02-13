using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

namespace QSB {
    class QSBNetworkManager: NetworkManager {
        void Awake () {
        }

        public override void OnClientConnect (NetworkConnection conn) {
            base.OnClientConnect(conn);

            QSB.Log("OnClientConnect");
        }
    }
}
