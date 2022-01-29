using QSB.WorldSync;

namespace QSB.QuantumSync.WorldObjects
{
	internal class QSBQuantumSocket : WorldObject<QuantumSocket>
	{
		public override bool ShouldDisplayDebug() => false;

		public override void SendInitialState(uint to) { }
	}
}
