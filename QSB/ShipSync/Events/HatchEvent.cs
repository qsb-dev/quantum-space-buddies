using OWML.Utils;
using QSB.Events;
using QSB.Messaging;

namespace QSB.ShipSync.Events
{
	class HatchEvent : QSBEvent<BoolMessage>
	{
		public override EventType Type => EventType.OpenHatch;

		public override void SetupListener() 
			=> GlobalMessenger<bool>.AddListener(EventNames.QSBHatchState, Handler);

		public override void CloseListener() 
			=> GlobalMessenger<bool>.RemoveListener(EventNames.QSBHatchState, Handler);

		private void Handler(bool open) => SendEvent(CreateMessage(open));

		private BoolMessage CreateMessage(bool open) => new BoolMessage
		{
			AboutId = LocalPlayerId,
			Value = open
		};

		public override void OnReceiveRemote(bool server, BoolMessage message)
		{
			var shipTransform = Locator.GetShipTransform();
			var hatchController = shipTransform.GetComponentInChildren<HatchController>();
			if (message.Value)
			{
				hatchController.Invoke("OpenHatch");
			}
			else
			{
				hatchController.Invoke("CloseHatch");
			}
		}
	}
}