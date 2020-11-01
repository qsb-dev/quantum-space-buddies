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
                    SwitchToInstrument(InstrumentType.REIBECK);
                }
                else
                {
                    CameraManager.Instance.SwitchTo1stPerson();
                    SwitchToInstrument(InstrumentType.NONE);
                }
            }
        }

        public void SwitchToInstrument(InstrumentType type)
        {
            PlayerRegistry.LocalPlayer.CurrentInstrument = type;
            GlobalMessenger<InstrumentType>.FireEvent(EventNames.QSBPlayInstrument, type);
        }
    }
}
