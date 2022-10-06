using QSB.Messaging;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.QuantumSync;

namespace QSB.Tools.ProbeLauncherTool.Messages;

internal class RemoveSnapshotMessage : QSBMessage
{
	static RemoveSnapshotMessage()
		=> GlobalMessenger.AddListener(OWEvents.ProbeSnapshotRemoved, Handle);

	private static void Handle()
	{
		if (PlayerTransformSync.LocalInstance)
		{
			new RemoveSnapshotMessage().Send();
		}
	}

	public override void OnReceiveLocal()
		=> QuantumManager.OnRemoveProbeSnapshot(QSBPlayerManager.LocalPlayer);

	public override void OnReceiveRemote()
		=> QuantumManager.OnRemoveProbeSnapshot(QSBPlayerManager.GetPlayer(From));
}
