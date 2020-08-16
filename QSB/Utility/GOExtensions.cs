using UnityEngine;

namespace QSB.Utility
{
    public static class GOExtensions
    {
        public static void Show(this GameObject gameObject) => SetRendererState(gameObject, true);

        public static void Hide(this GameObject gameObject) => SetRendererState(gameObject, false);

        private static void SetRendererState(GameObject gameObject, bool state)
        {
            var renderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var renderer in renderers)
            {
                renderer.enabled = state;
            }
        }
    }
}
