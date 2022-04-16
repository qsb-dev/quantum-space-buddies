using QSB.Messaging;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.ShipSync;
using QSB.Tools.ProbeLauncherTool.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.Tools.ProbeLauncherTool.Messages;

public class EquipProbeLauncherMessage : QSBWorldObjectMessage<QSBProbeLauncher, bool>
{
	static EquipProbeLauncherMessage()
	{
		GlobalMessenger<ProbeLauncher>.AddListener(OWEvents.ProbeLauncherEquipped, launcher => Handle(launcher, true));
		GlobalMessenger<ProbeLauncher>.AddListener(OWEvents.ProbeLauncherUnequipped, launcher => Handle(launcher, false));
	}

	private static void Handle(ProbeLauncher launcher, bool equipped)
	{
		if (PlayerTransformSync.LocalInstance == null)
		{
			return;
		}

		var local = launcher == QSBPlayerManager.LocalPlayer.LocalProbeLauncher;

		if (local)
		{
			new PlayerEquipLauncherMessage(equipped).Send();
			return;
		}

		var worldObject = launcher.GetWorldObject<QSBProbeLauncher>();
		worldObject.SendMessage(new EquipProbeLauncherMessage(equipped));
	}

	private EquipProbeLauncherMessage(bool equipped) : base(equipped) { }

	public override void OnReceiveRemote()
	{
		DebugLog.DebugWrite($"{From} equip {WorldObject}");

		var player = QSBPlayerManager.GetPlayer(From);
		player.ProbeLauncherEquipped = WorldObject;

		if (player.FlyingShip && WorldObject.AttachedObject == ShipManager.Instance.CockpitController.GetShipProbeLauncher())
		{
			ShipManager.Instance.UpdateProbeLauncher(Data);
		}
	}

	public override void OnReceiveLocal()
	{
		QSBPlayerManager.LocalPlayer.ProbeLauncherEquipped = WorldObject;
	}
}