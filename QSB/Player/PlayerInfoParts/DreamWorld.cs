using QSB.EchoesOfTheEye.DreamLantern;
using QSB.EchoesOfTheEye.DreamLantern.WorldObjects;
using QSB.ItemSync.WorldObjects.Items;

namespace QSB.Player;

public partial class PlayerInfo
{
	public bool InDreamWorld { get; set; }
	public QSBDreamLanternItem AssignedSimulationLantern { get; set; }
}
