namespace QSB.QuantumSync.WorldObjects;

public class QSBQuantumShuffleObject : QSBQuantumObject<QuantumShuffleObject>
{
	public override void SendInitialState(uint to)
	{
		base.SendInitialState(to);

		// todo SendInitialState
	}

	public void ShuffleObjects(int[] indexArray)
	{
		var shuffledObjects = AttachedObject._shuffledObjects;
		var localPositions = AttachedObject._localPositions;
		for (var i = 0; i < shuffledObjects.Length; i++)
		{
			shuffledObjects[i].localPosition = localPositions[indexArray[i]];
		}
	}
}