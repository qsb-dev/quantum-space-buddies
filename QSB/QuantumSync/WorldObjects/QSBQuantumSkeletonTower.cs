namespace QSB.QuantumSync.WorldObjects;

internal class QSBQuantumSkeletonTower : QSBQuantumObject<QuantumSkeletonTower>
{
	public override string ReturnLabel() => $"{base.ReturnLabel()}"
											+ $"{AttachedObject._index} {AttachedObject._waitForPlayerToLookAtTower}\n"
											+ $"{AttachedObject._waitForFlicker} {AttachedObject._flickering}";

	public void MoveSkeleton(int index)
	{
		AttachedObject._pointingSkeletons[index].gameObject.SetActive(false);

		AttachedObject._towerSkeletons[AttachedObject._index].SetActive(true);
		AttachedObject._index++;
		AttachedObject._waitForPlayerToLookAtTower = true;
	}
}