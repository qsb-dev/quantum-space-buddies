using QSB.AuthoritySync;
using QSB.Messaging;
using QSB.ModelShip.TransformSync;
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

	public override void OnReceiveLocal()
	{
		ModelShipManager.Instance.CurrentFlyer = Data
			? From
			: uint.MaxValue;

		if (QSBCore.IsHost)
		{
			ModelShipTransformSync.LocalInstance.netIdentity.SetAuthority(Data
				? From
				: QSBPlayerManager.LocalPlayerId);
		}

		// Client messes up its position when they start flying it
		// We can just recall it immediately so its in the right place.
		var console = QSBWorldSync.GetUnityObject<RemoteFlightConsole>();
		console.RespawnModelShip(false);
	}

	public override void OnReceiveRemote()
	{
		ModelShipManager.Instance.CurrentFlyer = Data
			? From
			: uint.MaxValue;

		var console = QSBWorldSync.GetUnityObject<RemoteFlightConsole>();

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

		if (QSBCore.IsHost)
		{
			ModelShipTransformSync.LocalInstance.netIdentity.SetAuthority(Data
				? From
				: QSBPlayerManager.LocalPlayerId);
		}
	}
}
