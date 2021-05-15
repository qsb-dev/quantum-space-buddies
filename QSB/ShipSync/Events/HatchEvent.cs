using OWML.Utils;
using QSB.Events;
using QSB.Messaging;

namespace QSB.ShipSync.Events
{
	internal class HatchEvent : QSBEvent<BoolMessage>
	{
		public override QSB.Events.EventType Type => QSB.Events.EventType.OpenHatch;

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
			if (message.Value)
			{
				ShipManager.Instance.HatchController.Invoke("OpenHatch");
			}
			else
			{
				ShipManager.Instance.ShipTractorBeam.DeactivateTractorBeam();
				ShipManager.Instance.HatchController.Invoke("CloseHatch");
			}
		}
	}
}