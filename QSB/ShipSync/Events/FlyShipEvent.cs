using OWML.Utils;
using QSB.Events;
using QSB.Messaging;
using QSB.Player;
using UnityEngine;

namespace QSB.ShipSync.Events
{
	class FlyShipEvent : QSBEvent<BoolMessage>
	{
		public override QSB.Events.EventType Type => QSB.Events.EventType.FlyShip;

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

		private BoolMessage CreateMessage(bool flying) => new BoolMessage
		{
			AboutId = LocalPlayerId,
			Value = flying
		};

		public override void OnReceiveLocal(bool server, BoolMessage message)
		{
			SetCurrentFlyer(message.Value, message.AboutId);
		}

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
		}
	}
}
