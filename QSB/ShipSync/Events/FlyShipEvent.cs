using OWML.Utils;
using QSB.AuthoritySync;
using QSB.Events;
using QSB.Messaging;
using QSB.Player;
using QSB.ShipSync.TransformSync;
using UnityEngine;

namespace QSB.ShipSync.Events
{
	internal class FlyShipEvent : QSBEvent<BoolMessage>
	{
		public override bool RequireWorldObjectsReady() => true;

		public override void SetupListener()
		{
			GlobalMessenger<OWRigidbody>.AddListener(EventNames.EnterFlightConsole, (OWRigidbody rigidbody) => Handler(true));
			GlobalMessenger.AddListener(EventNames.ExitFlightConsole, () => Handler(false));
		}

		public override void CloseListener()
		{
			GlobalMessenger<OWRigidbody>.RemoveListener(EventNames.EnterFlightConsole, (OWRigidbody rigidbody) => Handler(true));
			GlobalMessenger.RemoveListener(EventNames.ExitFlightConsole, () => Handler(false));
		}

		private void Handler(bool flying) => SendEvent(CreateMessage(flying));

		private BoolMessage CreateMessage(bool flying) => new()
		{
			AboutId = LocalPlayerId,
			Value = flying
		};

		public override void OnReceiveLocal(bool server, BoolMessage message) => SetCurrentFlyer(message.Value, message.AboutId);

		public override void OnReceiveRemote(bool server, BoolMessage message)
		{
			SetCurrentFlyer(message.Value, message.AboutId);
			var shipCockpitController = GameObject.Find("ShipCockpitController").GetComponent<ShipCockpitController>();
			var interactVolume = shipCockpitController.GetValue<SingleInteractionVolume>("_interactVolume");
			if (message.Value)
			{
				interactVolume.DisableInteraction();
			}
			else
			{
				interactVolume.EnableInteraction();
			}
		}

		private void SetCurrentFlyer(bool isFlying, uint id)
		{
			ShipManager.Instance.CurrentFlyer = isFlying
				? id
				: uint.MaxValue;

			if (QSBCore.IsHost)
			{
				ShipTransformSync.LocalInstance.NetIdentity.SetAuthority(isFlying
					? id
					: QSBPlayerManager.LocalPlayerId);
			}
		}
	}
}
