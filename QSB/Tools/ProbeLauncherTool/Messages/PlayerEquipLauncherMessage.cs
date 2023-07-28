using QSB.Messaging;
using QSB.Player;

namespace QSB.Tools.ProbeLauncherTool.Messages;

public class PlayerEquipLauncherMessage : QSBMessage<bool>
{
	public PlayerEquipLauncherMessage(bool equipped) : base(equipped) { }

	public override void OnReceiveRemote()
	{
		var player = QSBPlayerManager.GetPlayer(From);
		player.LocalProbeLauncherEquipped = Data;
		player.ProbeLauncherTool?.ChangeEquipState(Data);
	}

	public override void OnReceiveLocal()
	{
		QSBPlayerManager.LocalPlayer.LocalProbeLauncherEquipped = Data;
	}
}
