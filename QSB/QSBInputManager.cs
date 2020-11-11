using OWML.Common;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB
{
    public delegate void TauntEvent();

    public class QSBInputManager : MonoBehaviour
    {
        public static QSBInputManager Instance;
        public static event TauntEvent ChertTaunt;
        public static event TauntEvent EskerTaunt;
        public static event TauntEvent FeldsparTaunt;
        public static event TauntEvent GabbroTaunt;
        public static event TauntEvent RiebeckTaunt;
        public static event TauntEvent SolanumTaunt;

        public void Awake()
        {
            Instance = this;
            if (Input.GetKeyDown(KeyCode.T))
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
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
                    SolanmTaunt?.Invoke();
                }
            }
        }
    }
}
