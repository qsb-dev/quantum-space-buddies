using QSB.Player;
using System.Linq;

namespace QSB.QuantumSync.WorldObjects
{
	internal class QSBQuantumMoon : QSBQuantumObject<QuantumMoon>
	{
		public override void Init(QuantumMoon moonObject, int id)
		{
			ObjectId = id;
			AttachedObject = moonObject;
			ControllingPlayer = QSBCore.IsHost
				? QSBPlayerManager.LocalPlayerId
				: QSBPlayerManager.PlayerList.OrderBy(x => x.PlayerId).First().PlayerId;
			base.Init(moonObject, id);
		}
	}
}
