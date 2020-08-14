using UnityEngine;

namespace QSB.Tools
{
    public class QSBProbe : MonoBehaviour
    {
        public void SetState(bool state)
        {
            if (state)
            {
                Activate();
            }
            else
            {
                Deactivate();
            }
        }

        private void Activate()
        {
            gameObject.SetActive(true);
            var renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (var item in renderers)
            {
                item.enabled = true;
            }
        }

        private void Deactivate()
        {
            var renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (var item in renderers)
            {
                item.enabled = false;
            }
        }

    }
}
