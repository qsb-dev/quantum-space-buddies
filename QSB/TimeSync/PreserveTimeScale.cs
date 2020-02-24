using UnityEngine;

namespace QSB.TimeSync
{
    public class PreserveTimeScale : QSBBehaviour
    {
        private void Update()
        {
            if (IsPlayerAwake)
            {
                Time.timeScale = 1;
            }
        }
    }
}
