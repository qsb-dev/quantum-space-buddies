using QSB.Animation;
using QSB.EventsCore;
using QSB.Instruments.QSBCamera;
using QSB.Player;
using QSB.Utility;
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

            QSBInputManager.ChertTaunt += () => StartInstrument(AnimationType.Chert);
            QSBInputManager.EskerTaunt += () => StartInstrument(AnimationType.Esker);
            QSBInputManager.FeldsparTaunt += () => StartInstrument(AnimationType.Feldspar);
            QSBInputManager.GabbroTaunt += () => StartInstrument(AnimationType.Gabbro);
            QSBInputManager.RiebeckTaunt += () => StartInstrument(AnimationType.Riebeck);
            QSBInputManager.SolanumTaunt += () => StartInstrument(AnimationType.Solanum);
        }

        private void OnDestroy()
        {
            QSBInputManager.ChertTaunt -= () => StartInstrument(AnimationType.Chert);
            QSBInputManager.EskerTaunt -= () => StartInstrument(AnimationType.Esker);
            QSBInputManager.FeldsparTaunt -= () => StartInstrument(AnimationType.Feldspar);
            QSBInputManager.GabbroTaunt -= () => StartInstrument(AnimationType.Gabbro);
            QSBInputManager.RiebeckTaunt -= () => StartInstrument(AnimationType.Riebeck);
            QSBInputManager.SolanumTaunt -= () => StartInstrument(AnimationType.Solanum);
        }

        public void StartInstrument(AnimationType type)
        {
            if (QSBPlayerManager.LocalPlayer.PlayingInstrument)
            {
                return;
            }
            _savedType = QSBPlayerManager.LocalPlayer.Animator.CurrentType;
            CameraManager.Instance.SwitchTo3rdPerson();
            SwitchToType(type);
        }

        public void ReturnToPlayer()
        {
            if (!QSBPlayerManager.LocalPlayer.PlayingInstrument)
            {
                return;
            }
            CameraManager.Instance.SwitchTo1stPerson();
            SwitchToType(_savedType);
        }

        public void SwitchToType(AnimationType type)
        {
            GlobalMessenger<uint, AnimationType>.FireEvent(EventNames.QSBChangeAnimType, QSBPlayerManager.LocalPlayerId, type);
            QSBPlayerManager.LocalPlayer.Animator.SetAnimationType(type);
        }
    }
}
