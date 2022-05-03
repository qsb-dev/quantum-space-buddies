using QSB.WorldSync;

namespace QSB.Utility.LinkedWorldObject;

/// <summary>
/// a network behaviour that is linked to a world object
/// </summary>
public interface ILinkedNetworkBehaviour
{
	void SetWorldObject(IWorldObject worldObject);
}
