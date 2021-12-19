using QSB.Player;
using System.Linq;

namespace QSB.QuantumSync.WorldObjects
{
	internal class QSBQuantumMoon : QSBQuantumObject<QuantumMoon>
	{
		public override void Init()
		{
			// smallest player id is the host
			ControllingPlayer = QSBPlayerManager.PlayerList.Min(x => x.PlayerId);
			base.Init();
		}
	}
}
