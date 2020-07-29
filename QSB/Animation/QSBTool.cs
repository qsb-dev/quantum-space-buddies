using UnityEngine;

namespace QSB.Animation
{
    public class QSBTool : PlayerTool
    {
        public ToolType Type;
        public GameObject ToolGameObject;

        private void OnEnable()
        {
            ToolGameObject.SetActive(true);
        }

        private void OnDisable()
        {
            ToolGameObject.SetActive(false);
        }
    }
}