namespace QSB.QuantumSync.WorldObjects
{
	internal class QSBQuantumSkeletonTower : QSBQuantumObject<QuantumSkeletonTower>
	{
		public override string ReturnLabel() => $"{base.ReturnLabel()}\n"
			+ $"{AttachedObject._index} {AttachedObject._waitForPlayerToLookAtTower}\n"
			+ $"{AttachedObject._waitForFlicker} {AttachedObject._flickering}";

		public void MoveSkeleton(int pointingIndex, int towerIndex)
		{
			AttachedObject._pointingSkeletons[pointingIndex].gameObject.SetActive(false);

			AttachedObject._towerSkeletons[towerIndex].SetActive(true);
			AttachedObject._index = towerIndex + 1;
			AttachedObject._waitForPlayerToLookAtTower = true;
		}
	}
}
