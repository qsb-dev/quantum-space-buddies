using QSB.ItemSync.WorldObjects.Items;
using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.EchoesOfTheEye.DreamWorld.Messages;

internal class EnterDreamWorldMessage : QSBMessage<int>
{
	public EnterDreamWorldMessage(int lanternId) : base(lanternId) { }

	public override void OnReceiveRemote()
	{
		var player = QSBPlayerManager.GetPlayer(From);
		player.InDreamWorld = true;
		player.AssignedSimulationLantern = QSBWorldSync.GetWorldObject<QSBDreamLanternItem>(Data);
	}
}
