using QSB.Player;

namespace QSB.QuantumSync.WorldObjects
{
	internal class QSBEyeProxyQuantumMoon : QSBQuantumObject<EyeProxyQuantumMoon>
	{
		public override void Init()
		{
			ControllingPlayer = QSBPlayerManager.LocalPlayerId;
			base.Init();
		}
	}
}
