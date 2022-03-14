using QSB.Utility.VariableSync;
using QSB.WorldSync;

namespace QSB.Utility.LinkedWorldObject;

/// <summary>
/// helper implementation of the interface
/// </summary>
public abstract class LinkedVariableSyncer<T, TWorldObject> : BaseVariableSyncer<T>, ILinkedNetworkBehaviour<TWorldObject>
	where TWorldObject : IWorldObject
{
	public TWorldObject WorldObject { get; private set; }
	public void LinkTo(IWorldObject worldObject) => WorldObject = (TWorldObject)worldObject;
}
