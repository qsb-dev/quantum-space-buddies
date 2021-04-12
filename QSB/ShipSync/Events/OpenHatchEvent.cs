using OWML.Utils;
using QSB.Events;
using QSB.Messaging;

namespace QSB.ShipSync.Events
{
	class OpenHatchEvent : QSBEvent<PlayerMessage>
	{
		public override EventType Type => EventType.OpenHatch;

		public override void SetupListener() 
			=> GlobalMessenger.AddListener(EventNames.QSBOpenHatch, Handler);

		public override void CloseListener() 
			=> GlobalMessenger.RemoveListener(EventNames.QSBOpenHatch, Handler);

		private void Handler() => SendEvent(CreateMessage());

		private PlayerMessage CreateMessage() => new PlayerMessage
		{
			AboutId = LocalPlayerId
		};

		public override void OnReceiveRemote(bool server, PlayerMessage message)
		{
			var shipTransform = Locator.GetShipTransform();
			var hatchController = shipTransform.GetComponentInChildren<HatchController>();
			hatchController.Invoke("OpenHatch");
		}
	}
}