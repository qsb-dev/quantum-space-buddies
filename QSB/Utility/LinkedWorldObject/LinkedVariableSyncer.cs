using QSB.Utility.VariableSync;
using QSB.WorldSync;

namespace QSB.Utility.LinkedWorldObject;

/// <summary>
/// helper implementation of the interface
/// </summary>
public abstract class LinkedVariableSyncer<T, TWorldObject> : BaseVariableSyncer<T>, ILinkedNetworkBehaviour
	where TWorldObject : IWorldObject
{
	protected TWorldObject WorldObject { get; private set; }
	public void SetWorldObject(IWorldObject worldObject) => WorldObject = (TWorldObject)worldObject;
}
