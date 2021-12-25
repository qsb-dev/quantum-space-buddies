using QSB.Messaging;

namespace QSB.SatelliteSync.Messages
{
	internal class SatelliteProjectorMessage : QSBBoolMessage
	{
		public SatelliteProjectorMessage(bool usingProjector) => Value = usingProjector;

		public SatelliteProjectorMessage() { }

		public override void OnReceiveRemote()
		{
			if (Value)
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
