using QSB.ItemSync.WorldObjects.Items;

namespace QSB.Player;

public partial class PlayerInfo
{
	public bool InDreamWorld { get; set; }
	public QSBDreamLanternItem AssignedSimulationLantern { get; set; }
}
