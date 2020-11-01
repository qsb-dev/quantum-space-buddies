using QSB.Events;
using QSB.Instruments.QSBCamera;
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
                if (!PlayerRegistry.LocalPlayer.PlayingInstrument)
                {
                    CameraManager.Instance.SwitchTo3rdPerson();
                    GlobalMessenger<InstrumentType, bool>.FireEvent(EventNames.QSBPlayInstrument, InstrumentType.REIBECK, true);
                }
                else
                {
                    CameraManager.Instance.SwitchTo1stPerson();
                    GlobalMessenger<InstrumentType, bool>.FireEvent(EventNames.QSBPlayInstrument, InstrumentType.REIBECK, false);
                }
            }
        }
    }
}
