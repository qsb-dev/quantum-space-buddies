using QSB.WorldSync;

namespace QSB.QuantumSync.WorldObjects
{
	internal class QSBQuantumState : WorldObject<QuantumState>
	{
		public bool IsMeantToBeEnabled;

		public void SetVisible(bool visible)
		{
			IsMeantToBeEnabled = visible;
			AttachedObject.SetVisible(visible);
		}

		public override bool ShouldDisplayDebug() => false;

		public override void SendInitialState(uint to) { }
	}
}
