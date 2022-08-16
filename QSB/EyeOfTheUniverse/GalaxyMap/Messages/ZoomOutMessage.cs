using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.EyeOfTheUniverse.GalaxyMap.Messages;

internal class ZoomOutMessage : QSBMessage
{
	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveRemote()
	{
		var controller = QSBWorldSync.GetUnityObject<GalaxyMapController>();
		controller.enabled = true;
		Locator.GetPlayerController().SetColliderActivation(false);
		controller._endlessObservatoryVolume.SetActivation(false);
		controller._forestOfGalaxiesVolume.SetTriggerActivation(false);
		ReticleController.Hide();
		controller._zoomSpeed = 50f;
		Locator.GetPlayerBody().AddVelocityChange(-Locator.GetPlayerCamera().transform.forward * controller._zoomSpeed);
		controller._origEyePos = controller._eyeTransform.localPosition;
		controller._audioSource.Play();
		RumbleManager.PlayGalaxyZoom();
		Locator.GetEyeStateManager().SetState(EyeState.ZoomOut);
	}
}