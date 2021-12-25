using QSB.Messaging;

namespace QSB.SatelliteSync.Messages
{
	internal class SatelliteProjectorSnapshotMessage : QSBBoolMessage
	{
		public SatelliteProjectorSnapshotMessage(bool forward) => Value = forward;

		public SatelliteProjectorSnapshotMessage() { }

		public override void OnReceiveRemote() => SatelliteProjectorManager.Instance.RemoteTakeSnapshot(Value);
	}
}
