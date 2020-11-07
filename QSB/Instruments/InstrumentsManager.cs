using QSB.Animation;
using QSB.EventsCore;
using QSB.Instruments.QSBCamera;
using QSB.Player;
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
                if (!QSBPlayerManager.LocalPlayer.PlayingInstrument)
                {
                    CameraManager.Instance.SwitchTo3rdPerson();
                    SwitchToInstrument(InstrumentType.RIEBECK);
                    QSBPlayerManager.GetSyncObject<AnimationSync>(QSBPlayerManager.LocalPlayerId).SetAnimationType(AnimationType.Chert);
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
            QSBPlayerManager.LocalPlayer.CurrentInstrument = type;
            GlobalMessenger<InstrumentType>.FireEvent(EventNames.QSBPlayInstrument, type);
        }
    }
}
