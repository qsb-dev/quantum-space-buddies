using QSB.WorldSync;

namespace QSB.QuantumSync.WorldObjects;

public class QSBQuantumState : WorldObject<QuantumState>
{
	public bool IsMeantToBeEnabled;

	public void SetVisible(bool visible)
	{
		IsMeantToBeEnabled = visible;
		AttachedObject.SetVisible(visible);
	}

	public override bool ShouldDisplayDebug() => false;
}