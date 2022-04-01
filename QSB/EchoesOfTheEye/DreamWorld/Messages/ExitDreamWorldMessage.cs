using QSB.Messaging;
using QSB.Player;
using QSB.Player.TransformSync;

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
	}

	public override void OnReceiveRemote()
	{
		var player = QSBPlayerManager.GetPlayer(From);
		player.InDreamWorld = false;
		player.AssignedSimulationLantern = null;
	}
}
