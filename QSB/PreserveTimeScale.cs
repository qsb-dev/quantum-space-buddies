using UnityEngine;

namespace QSB {
    class PreserveTimeScale: QSBBehaviour {
        void Update () {
            if (isPlayerAwake) {
                Time.timeScale = 1;
            }
        }
    }
}
