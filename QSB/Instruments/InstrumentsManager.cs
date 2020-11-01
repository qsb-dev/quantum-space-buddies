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
                CameraManager.Instance.ToggleViewMode();
            }
        }
    }
}
