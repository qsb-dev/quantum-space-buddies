using QSB.Player;
using QSB.Utility;
using System.Collections;
using UnityEngine;

namespace QSB.StatueSync
{
	class StatueManager : MonoBehaviour
	{
		public static StatueManager Instance { get; private set; }

		private void Awake() 
			=> Instance = this;

		public void BeginSequence(Vector3 position, Quaternion rotation, float cameraDegrees) 
			=> StartCoroutine(BeginRemoteUplinkSequence(position, rotation, cameraDegrees));

		private IEnumerator BeginRemoteUplinkSequence(Vector3 position, Quaternion rotation, float cameraDegrees)
		{
			var cameraEffectController = Locator.GetPlayerCamera().GetComponent<PlayerCameraEffectController>();
			cameraEffectController.CloseEyes(0.5f);
			OWInput.ChangeInputMode(InputMode.None);
			Locator.GetPauseCommandListener().AddPauseCommandLock();
			Locator.GetToolModeSwapper().UnequipTool();
			Locator.GetFlashlight().TurnOff(false);
			yield return new WaitForSeconds(0.5f);
			// go to position
			QSBPlayerManager.HideAllPlayers();
			var timberHearth = Locator.GetAstroObject(AstroObject.Name.TimberHearth).GetAttachedOWRigidbody();
			Locator.GetPlayerBody().transform.position = timberHearth.transform.TransformPoint(position);
			Locator.GetPlayerBody().transform.rotation = timberHearth.transform.rotation * rotation;
			Locator.GetPlayerCamera().GetComponent<PlayerCameraController>().SetDegreesY(cameraDegrees);
			cameraEffectController.OpenEyes(1f, true);
			var uplinkTrigger = FindObjectOfType<MemoryUplinkTrigger>();
			uplinkTrigger.StartCoroutine("BeginUplinkSequence");
			yield break;
		}
	}
}
