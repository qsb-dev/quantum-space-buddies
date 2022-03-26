using JetBrains.Annotations;
using QSB.ItemSync.WorldObjects.Items;
using QSB.Messaging;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.DreamWorld.Messages;

/// <summary>
/// todo initial state
/// </summary>
[UsedImplicitly]
internal class EnterDreamWorldMessage : QSBWorldObjectMessage<QSBDreamLanternItem>
{
	static EnterDreamWorldMessage()
	{
		GlobalMessenger.AddListener(OWEvents.EnterDreamWorld, () =>
		{
			if (!PlayerTransformSync.LocalInstance)
			{
				return;
			}

			Locator.GetDreamWorldController()
				.GetPlayerLantern()
				.GetWorldObject<QSBDreamLanternItem>()
				.SendMessage(new EnterDreamWorldMessage());
		});
	}

	public override void OnReceiveRemote()
	{
		var player = QSBPlayerManager.GetPlayer(From);
		player.InDreamWorld = true;
		player.AssignedSimulationLantern = WorldObject;
	}
}
