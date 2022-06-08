using QSB.AuthoritySync;
using QSB.Messaging;
using QSB.ModelShip.TransformSync;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.WorldSync;

namespace QSB.ModelShip.Messages;

internal class UseFlightConsole : QSBMessage<bool>
{
	static UseFlightConsole()
	{
		GlobalMessenger<OWRigidbody>.AddListener(OWEvents.EnterRemoteFlightConsole, _ => Handler(true));
		GlobalMessenger.AddListener(OWEvents.ExitRemoteFlightConsole, () => Handler(false));
	}

	private static void Handler(bool active)
	{
		if (PlayerTransformSync.LocalInstance != null)
		{
			new UseFlightConsole(active).Send();
		}
	}

	private UseFlightConsole(bool active) : base(active) { }

	public override void OnReceiveLocal()
	{
		if (QSBCore.IsHost)
		{
			ModelShipTransformSync.LocalInstance.netIdentity.SetAuthority(Data
				? From
				: QSBPlayerManager.LocalPlayerId);
		}
	}

	public override void OnReceiveRemote()
	{
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

		if (QSBCore.IsHost)
		{
			ModelShipTransformSync.LocalInstance.netIdentity.SetAuthority(Data
				? From
				: QSBPlayerManager.LocalPlayerId);
		}
	}
}
