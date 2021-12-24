using System.Linq;
using QSB.Events;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.EyeOfTheUniverse.GalaxyMap.Messages
{
	internal class ZoomOutEvent : QSBEvent<PlayerMessage>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener() => GlobalMessenger.AddListener(EventNames.QSBZoomOut, Handler);
		public override void CloseListener() => GlobalMessenger.RemoveListener(EventNames.QSBZoomOut, Handler);

		private void Handler() => SendEvent(CreateMessage());

		private PlayerMessage CreateMessage() => new()
		{
			AboutId = LocalPlayerId
		};

		public override void OnReceiveRemote(bool isHost, PlayerMessage message)
		{
			var controller = QSBWorldSync.GetUnityObjects<GalaxyMapController>().First();
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
}
