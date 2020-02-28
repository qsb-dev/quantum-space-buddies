using OWML.ModHelper.Events;
using UnityEngine;

namespace QSB.TimeSync
{
    public class PreserveTimeScale : MonoBehaviour
    {
        private void Start()
        {
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
