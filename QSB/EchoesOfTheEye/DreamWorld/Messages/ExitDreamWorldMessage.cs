using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.DreamWorld.Messages;

/// <summary>
/// todo SendInitialState
/// </summary>
internal class ExitDreamWorldMessage : QSBMessage
{
	static ExitDreamWorldMessage()
	{
		GlobalMessenger.AddListener(OWEvents.ExitDreamWorld, () =>
		{
			if (!PlayerTransformSync.LocalInstance)
			{
				return;
			}

			new ExitDreamWorldMessage().Send();
		});
	}

	public override void OnReceiveLocal()
	{
		var player = QSBPlayerManager.LocalPlayer;
		player.InDreamWorld = false;
		player.AssignedSimulationLantern = null;

		foreach (var ghost in QSBWorldSync.GetWorldObjects<QSBGhostBrain>())
		{
			ghost.OnExitDreamWorld(player);
		}
	}

	public override void OnReceiveRemote()
	{
		var player = QSBPlayerManager.GetPlayer(From);
		player.InDreamWorld = false;
		player.AssignedSimulationLantern = null;

		if (QSBCore.IsHost)
		{
			foreach (var ghost in QSBWorldSync.GetWorldObjects<QSBGhostBrain>())
			{
				ghost.OnExitDreamWorld(player);
				ghost.GetEffects().OnSectorOccupantsUpdated();
			}
		}

		Locator.GetAlarmSequenceController().OnExitDreamWorld();
	}
}
