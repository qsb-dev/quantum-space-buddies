using QSB.WorldSync;

namespace QSB.QuantumSync.WorldObjects
{
	internal class QSBQuantumShuffleObject : WorldObject<QuantumShuffleObject>
	{
		public override void Init(QuantumShuffleObject shuffleObject, int id)
		{
			ObjectId = id;
			AttachedObject = shuffleObject;
		}
	}
}
