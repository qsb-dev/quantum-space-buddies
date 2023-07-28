using QSB.Messaging;
using QSB.Player;
using QSB.QuantumSync;
using QSB.WorldSync;

namespace QSB.Tools.ProbeLauncherTool.Messages;

public class PlayerLauncherTakeSnapshotMessage : QSBMessage<ProbeCamera.ID>
{
	public PlayerLauncherTakeSnapshotMessage(ProbeCamera.ID cameraId) : base(cameraId) { }

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveLocal() => QuantumManager.OnTakeProbeSnapshot(QSBPlayerManager.LocalPlayer, Data);

	public override void OnReceiveRemote()
	{
		var player = QSBPlayerManager.GetPlayer(From);
		player.ProbeLauncherTool.TakeSnapshot();
		QuantumManager.OnTakeProbeSnapshot(player, Data);
	}
}
