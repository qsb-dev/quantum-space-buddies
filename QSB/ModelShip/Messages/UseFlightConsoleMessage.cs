using QSB.Messaging;
using QSB.ModelShip.TransformSync;
using QSB.OwnershipSync;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.ModelShip.Messages;

internal class UseFlightConsoleMessage : QSBMessage<bool>
{
	static UseFlightConsoleMessage()
	{
		GlobalMessenger<OWRigidbody>.AddListener(OWEvents.EnterRemoteFlightConsole, _ => Handler(true));
		GlobalMessenger.AddListener(OWEvents.ExitRemoteFlightConsole, () => Handler(false));
	}

	private static void Handler(bool active)
	{
		if (PlayerTransformSync.LocalInstance != null)
		{
			new UseFlightConsoleMessage(active).Send();
		}
	}

	private UseFlightConsoleMessage(bool active) : base(active) { }

	public override void OnReceiveLocal() => SetCurrentFlyer(From, Data);

	public override void OnReceiveRemote()
	{
		var console = QSBWorldSync.GetUnityObject<RemoteFlightConsole>();

		SetCurrentFlyer(From, Data);

		if (Data)
		{
			console._modelShipBody.Unsuspend();
			console._interactVolume.ResetInteraction();
			console._interactVolume.DisableInteraction();
		}
		else
		{
			console._interactVolume.ResetInteraction();

			if (console._modelShipBody == null)
			{
				console._interactVolume.DisableInteraction();
				return;
			}

			console._modelShipBody.Suspend(console._suspensionBody);
			console._interactVolume.EnableInteraction();
		}

		QSBWorldSync.GetUnityObject<ModelShipController>()._detector.SetActive(Data);
		QSBWorldSync.GetUnityObjects<ModelShipLandingSpot>().ForEach(x => x._owCollider.SetActivation(Data));
	}

	private void SetCurrentFlyer(uint flyer, bool isFlying)
	{
		ModelShipManager.Instance.CurrentFlyer = isFlying
			? flyer
			: uint.MaxValue;

		if (QSBCore.IsHost)
		{
			ModelShipTransformSync.LocalInstance.netIdentity.SetOwner(isFlying
				? flyer
				: QSBPlayerManager.LocalPlayerId); // Host gets ownership when its not in use
		}

		// Client messes up its position when they start flying it
		// We can just recall it immediately so its in the right place.
		var console = QSBWorldSync.GetUnityObject<RemoteFlightConsole>();
		if (console._modelShipBody) // for when model ship is destroyed
		{
			console.RespawnModelShip(false);
		}
	}
}
