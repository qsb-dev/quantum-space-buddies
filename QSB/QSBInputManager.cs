using OWML.Common;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB
{
    public delegate void InputEvent();

    public class QSBInputManager : MonoBehaviour
    {
        public static QSBInputManager Instance;
        public static event InputEvent ChertTaunt;
        public static event InputEvent EskerTaunt;
        public static event InputEvent FeldsparTaunt;
        public static event InputEvent GabbroTaunt;
        public static event InputEvent RiebeckTaunt;
        public static event InputEvent SolanumTaunt;
        public static event InputEvent ExitTaunt;

        public void Awake()
        {
            Instance = this;
        }

        public void Update()
        {
            if (Input.GetKey(KeyCode.T))
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    DebugLog.DebugWrite("chert");
                    ChertTaunt?.Invoke();
                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    EskerTaunt?.Invoke();
                }
                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    FeldsparTaunt?.Invoke();
                }
                if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    GabbroTaunt?.Invoke();
                }
                if (Input.GetKeyDown(KeyCode.Alpha5))
                {
                    RiebeckTaunt?.Invoke();
                }
                if (Input.GetKeyDown(KeyCode.Alpha6))
                {
                    SolanumTaunt?.Invoke();
                }
            }

            if (OWInput.GetValue(InputLibrary.moveXZ, InputMode.None) != Vector2.zero 
                || OWInput.GetValue(InputLibrary.jump, InputMode.None) != 0f)
            {
                ExitTaunt?.Invoke();
            }
        }
    }
}
