using QSB.Animation;
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
                    SwitchToInstrument(AnimationType.Chert);
                }
                else
                {
                    CameraManager.Instance.SwitchTo1stPerson();
                    SwitchToInstrument(AnimationType.PlayerUnsuited);
                }
            }
        }

        public void SwitchToInstrument(AnimationType type)
        {
            QSBPlayerManager.GetSyncObject<AnimationSync>(QSBPlayerManager.LocalPlayerId).SetAnimationType(type);
        }
    }
}
