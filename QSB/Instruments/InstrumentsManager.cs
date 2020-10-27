using QSB.Instruments.QSBCamera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.Instruments
{
    public class InstrumentsManager : MonoBehaviour
    {
        public static InstrumentsManager Instance;

        private void Awake()
        {
            Instance = this;
            gameObject.AddComponent<CameraManager>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad9))
            {
                if (CameraManager.Instance.Mode == CameraMode.FirstPerson)
                {
                    CameraManager.Instance.SwitchTo3rdPerson();
                }
                else
                {
                    CameraManager.Instance.SwitchTo1stPerson();
                }
            }
        }
    }
}
