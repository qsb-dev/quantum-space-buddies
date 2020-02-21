using UnityEngine;

namespace QSB
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
