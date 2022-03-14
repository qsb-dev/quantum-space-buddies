using Mirror;
using QSB.WorldSync;

namespace QSB.Utility.WorldObjectLink;

/// <summary>
/// a world object that is linked to a network behaviour
/// </summary>
public interface ILinkedWorldObject<out TNetworkBehaviour> : IWorldObject
	where TNetworkBehaviour : NetworkBehaviour
{
	TNetworkBehaviour NetworkBehaviour { get; }
	void SetNetworkBehaviour(NetworkBehaviour networkBehaviour);
}
