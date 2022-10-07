using QSB.Messaging;
using QSB.Player;
using QSB.QuantumSync;
using QSB.Tools.ProbeLauncherTool.WorldObjects;

namespace QSB.Tools.ProbeLauncherTool.Messages;

internal class TakeSnapshotMessage : QSBWorldObjectMessage<QSBProbeLauncher, ProbeCamera.ID>
{
	public TakeSnapshotMessage(ProbeCamera.ID cameraId) : base(cameraId) { }

	public override void OnReceiveLocal()
		=> QuantumManager.OnTakeProbeSnapshot(QSBPlayerManager.LocalPlayer, Data);

	public override void OnReceiveRemote()
		=> WorldObject.TakeSnapshot(QSBPlayerManager.GetPlayer(From), Data);
}
