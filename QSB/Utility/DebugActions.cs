using UnityEngine;

namespace QSB.Utility
{
    public class DebugActions : MonoBehaviour
    {
        private void GoToVessel()
        {
            var spawnPoint = GameObject.Find("Spawn_Vessel").GetComponent<SpawnPoint>();

            var playerBody = Locator.GetPlayerBody();
            playerBody.WarpToPositionRotation(spawnPoint.transform.position, spawnPoint.transform.rotation);
            playerBody.SetVelocity(spawnPoint.GetPointVelocity());
        }

        private void InsertWarpCore()
        {
            var warpCore = GameObject.Find("Prefab_NOM_WarpCoreVessel").GetComponent<WarpCoreItem>();
            var socket = GameObject.Find("Interactibles_VesselBridge").GetComponentInChildren<WarpCoreSocket>();
            socket.PlaceIntoSocket(warpCore);

            GetComponent<NomaiCoordinateInterface>().SetPillarRaised(true, true);
        }

        private void Update()
        {
            if (!QSB.DebugMode)
            {
                return;
            }
            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                GoToVessel();
            }
            if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                InsertWarpCore();
            }
            if (Input.GetKeyDown(KeyCode.Keypad3))
            {
                GlobalMessenger.FireEvent("DebugWarpVessel");
            }
        }
    }
}