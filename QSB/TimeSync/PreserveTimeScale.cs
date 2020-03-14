using OWML.ModHelper.Events;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.TimeSync
{
    public class PreserveTimeScale : NetworkBehaviour
    {
        private void Start()
        {
            QSB.Helper.Menus.PauseMenu.GetButton("Button-EndCurrentLoop").Hide();

            if (isServer)
            {
                return;
            }

            var campfires = GameObject.FindObjectsOfType<Campfire>();
            foreach (var campfire in campfires)
            {
                campfire.SetValue("_canSleepHere", false);
            }
        }

    }
}
