using QSB.Events;
using QSB.Messaging;

namespace QSB.SatelliteSync.Events
{
	internal class SatelliteProjectorEvent : QSBEvent<BoolMessage>
	{
		public override EventType Type => EventType.SatelliteProjector;

		public override void SetupListener()
		{
			GlobalMessenger.AddListener(EventNames.QSBEnterSatelliteCamera, () => Handler(true));
			GlobalMessenger.AddListener(EventNames.QSBExitSatelliteCamera, () => Handler(false));
		}

		public override void CloseListener()
		{
			GlobalMessenger.RemoveListener(EventNames.QSBEnterSatelliteCamera, () => Handler(true));
			GlobalMessenger.RemoveListener(EventNames.QSBExitSatelliteCamera, () => Handler(false));
		}

		private void Handler(bool usingProjector) => SendEvent(CreateMessage(usingProjector));

		private BoolMessage CreateMessage(bool usingProjector) => new BoolMessage
		{
			AboutId = LocalPlayerId,
			Value = usingProjector
		};

		public override void OnReceiveRemote(bool isHost, BoolMessage message)
		{
			if (message.Value)
			{
				SatelliteProjectorManager.Instance.RemoteEnter();
			}
			else
			{
				SatelliteProjectorManager.Instance.RemoteExit();
			}
		}
	}
}
