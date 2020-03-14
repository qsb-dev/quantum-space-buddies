using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB
{
    class DebugActions : MonoBehaviour
    {
        void GoToVessel()
        {
            var spawnPoint = GameObject.Find("Spawn_Vessel").GetComponent<SpawnPoint>();

            OWRigidbody playerBody = Locator.GetPlayerBody();
            playerBody.WarpToPositionRotation(spawnPoint.transform.position, spawnPoint.transform.rotation);
            playerBody.SetVelocity(spawnPoint.GetPointVelocity());
        }

        void InsertWarpCore()
        {
            var warpCore = GameObject.Find("Prefab_NOM_WarpCoreVessel").GetComponent<WarpCoreItem>();
            var socket = GameObject.Find("Interactibles_VesselBridge").GetComponentInChildren<WarpCoreSocket>();
            socket.PlaceIntoSocket(warpCore);

            GetComponent<NomaiCoordinateInterface>().SetPillarRaised(true, true);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                GoToVessel();
            }
            if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                InsertWarpCore();
            }
        }
    }
}
