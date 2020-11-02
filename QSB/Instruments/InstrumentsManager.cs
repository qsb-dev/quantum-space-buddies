using QSB.Events;
using QSB.Instruments.QSBCamera;
using QSB.Utility;
using UnityEngine;

namespace QSB.Instruments
{
    public class InstrumentsManager : MonoBehaviour
    {
        public static InstrumentsManager Instance;
        private RuntimeAnimatorController RiebeckController;
        private RuntimeAnimatorController ChertController;
        private RuntimeAnimatorController GabbroController;
        private RuntimeAnimatorController FeldsparController;

        private void Awake()
        {
            Instance = this;
            gameObject.AddComponent<CameraManager>();
            QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(OWScene scene, bool inUniverse)
        {
            var reibeckRoot = GameObject.Find("Traveller_HEA_Riebeck_ANIM_Talking");
            RiebeckController = reibeckRoot.GetComponent<Animator>().runtimeAnimatorController;
            var chertRoot = GameObject.Find("Traveller_HEA_Chert_ANIM_Chatter_Chipper");
            ChertController = chertRoot.GetComponent<Animator>().runtimeAnimatorController;
            var gabbroRoot = GameObject.Find("Traveller_HEA_Gabbro_ANIM_IdleFlute");
            GabbroController = gabbroRoot.GetComponent<Animator>().runtimeAnimatorController;
            var feldsparRoot = GameObject.Find("Traveller_HEA_Feldspar_ANIM_Talking");
            FeldsparController = feldsparRoot.GetComponent<Animator>().runtimeAnimatorController;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad9))
            {
                if (!PlayerRegistry.LocalPlayer.PlayingInstrument)
                {
                    CameraManager.Instance.SwitchTo3rdPerson();
                    SwitchToInstrument(InstrumentType.RIEBECK);
                    var animator = Locator.GetPlayerTransform().Find("Traveller_HEA_Player_v2").GetComponent<Animator>();
                    animator.runtimeAnimatorController = ChertController;
                    animator.SetTrigger("Playing");
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
