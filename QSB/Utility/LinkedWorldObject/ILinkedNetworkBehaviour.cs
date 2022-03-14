using QSB.WorldSync;

namespace QSB.Utility.LinkedWorldObject;

/// <summary>
/// a network behaviour that is linked to a world object
/// </summary>
public interface ILinkedNetworkBehaviour<out TWorldObject> : INetworkBehaviour
	where TWorldObject : IWorldObject
{
	TWorldObject WorldObject { get; }
	void LinkTo(IWorldObject worldObject);
}
