using QSB.Messaging;

namespace QSB.SatelliteSync.Messages;

public class SatelliteProjectorSnapshotMessage : QSBMessage<bool>
{
	public SatelliteProjectorSnapshotMessage(bool forward) : base(forward) { }

	public override void OnReceiveRemote() => SatelliteProjectorManager.Instance.RemoteTakeSnapshot(Data);
}