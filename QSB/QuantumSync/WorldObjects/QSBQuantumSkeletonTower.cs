using QSB.Player;
using System.Linq;

namespace QSB.QuantumSync.WorldObjects
{
	internal class QSBQuantumSkeletonTower : QSBQuantumObject<QuantumSkeletonTower>
	{
		public override void Init()
		{
			// smallest player id is the host
			ControllingPlayer = QSBPlayerManager.PlayerList.Min(x => x.PlayerId);
			base.Init();
		}

		public override string ReturnLabel() => $"{base.ReturnLabel()}\n"
			+ $"{AttachedObject._index} {AttachedObject._waitForPlayerToLookAtTower}\n"
			+ $"{AttachedObject._waitForFlicker} {AttachedObject._flickering}";
	}
}
