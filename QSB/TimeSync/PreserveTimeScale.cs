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

            GlobalMessenger.AddListener("GamePaused", OnPause);

            var campfires = GameObject.FindObjectsOfType<Campfire>();
            foreach (var campfire in campfires)
            {
                campfire.SetValue("_canSleepHere", false);
            }
        }

        private void OnPause()
        {
            Time.timeScale = 1;
        }

    }
}
