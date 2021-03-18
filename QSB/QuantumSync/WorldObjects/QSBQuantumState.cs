using QSB.WorldSync;

namespace QSB.QuantumSync.WorldObjects
{
	class QSBQuantumState : WorldObject<QuantumState>
	{
		public bool IsMeantToBeEnabled;

		public override void Init(QuantumState state, int id)
		{
			ObjectId = id;
			AttachedObject = state;
		}

		public void SetVisible(bool visible)
		{
			IsMeantToBeEnabled = visible;
			AttachedObject.SetVisible(visible);
		}
	}
}
