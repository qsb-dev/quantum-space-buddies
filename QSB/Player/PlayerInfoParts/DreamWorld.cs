using QSB.ItemSync.WorldObjects.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.Player;

public partial class PlayerInfo
{
	public bool InDreamWorld { get; set; }
	public QSBDreamLanternItem AssignedSimulationLantern { get; set; }
}
