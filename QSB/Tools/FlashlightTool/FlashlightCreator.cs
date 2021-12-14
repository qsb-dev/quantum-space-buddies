using QSB.Player;
using UnityEngine;

namespace QSB.Tools.FlashlightTool
{
	internal static class FlashlightCreator
	{
		private static readonly Vector3 FlashlightOffset = new(0.7196316f, -0.2697681f, 0.3769455f);

		internal static void CreateFlashlight(PlayerInfo player)
		{
			var flashlightRoot = Object.Instantiate(GameObject.Find("FlashlightRoot"));
			flashlightRoot.name = "REMOTE_FlashlightRoot";
			flashlightRoot.SetActive(false);
			var oldComponent = flashlightRoot.GetComponent<Flashlight>();
			var component = flashlightRoot.AddComponent<QSBFlashlight>();

			component.Init(oldComponent);
			oldComponent.enabled = false;

			flashlightRoot.transform.parent = player.CameraBody.transform;
			flashlightRoot.transform.localPosition = FlashlightOffset;
			flashlightRoot.SetActive(true);
		}
	}
}
