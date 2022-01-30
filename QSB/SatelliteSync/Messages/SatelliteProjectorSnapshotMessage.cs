using QSB.Messaging;

namespace QSB.SatelliteSync.Messages
{
	internal class SatelliteProjectorSnapshotMessage : QSBMessage<bool>
	{
		public SatelliteProjectorSnapshotMessage(bool forward) => Value = forward;

		public override void OnReceiveRemote() => SatelliteProjectorManager.Instance.RemoteTakeSnapshot(Value);
	}
}
