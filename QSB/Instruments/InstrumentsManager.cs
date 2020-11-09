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
        private AnimationType _savedType;

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
                    _savedType = QSBPlayerManager.LocalPlayer.Animator.CurrentType;
                    CameraManager.Instance.SwitchTo3rdPerson();
                    SwitchToType(AnimationType.Chert);
                }
                else
                {
                    CameraManager.Instance.SwitchTo1stPerson();
                    SwitchToType(_savedType);
                }
            }
        }

        public void SwitchToType(AnimationType type)
        {
            GlobalMessenger<uint, AnimationType>.FireEvent(EventNames.QSBChangeAnimType, QSBPlayerManager.LocalPlayerId, type);
            QSBPlayerManager.LocalPlayer.Animator.SetAnimationType(type);
        }
    }
}
