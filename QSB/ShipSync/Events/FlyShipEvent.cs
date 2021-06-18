using OWML.Utils;
using QSB.Events;
using QSB.Messaging;
using QSB.Player;
using QSB.ShipSync.TransformSync;
using QSB.Utility;
using QuantumUNET;
using System.Linq;
using UnityEngine;

namespace QSB.ShipSync.Events
{
	internal class FlyShipEvent : QSBEvent<BoolMessage>
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

			if (QSBCore.IsServer)
			{
				var newAuthority = ShipManager.Instance.CurrentFlyer == uint.MaxValue
					? QNetworkServer.connections.First(x => x.GetPlayerId() == QSBPlayerManager.LocalPlayerId)
					: QNetworkServer.connections.First(x => x.GetPlayerId() == id);

				var ship = ShipTransformSync.LocalInstance;
				var shipNetId = ship.NetIdentity;

				if (shipNetId.ClientAuthorityOwner == newAuthority)
				{
					return;
				}

				if (shipNetId.ClientAuthorityOwner != null && shipNetId.ClientAuthorityOwner != newAuthority)
				{
					shipNetId.RemoveClientAuthority(shipNetId.ClientAuthorityOwner);
				}

				shipNetId.AssignClientAuthority(newAuthority);
			}
		}
	}
}
