using Mirror;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.Utility.WorldObjectLink;

/// <summary>
/// sent from the host to a non-host
/// telling a world object and network behaviour to link
/// </summary>
public class LinkWorldObjectMessage : QSBWorldObjectMessage<ILinkedWorldObject<NetworkBehaviour>, uint>
{
	public LinkWorldObjectMessage(NetworkBehaviour networkBehaviour) :
		base(networkBehaviour.netId) { }

	public override void OnReceiveRemote()
	{
		var networkBehaviour = NetworkClient.spawned[Data].GetComponent<ILinkedNetworkBehaviour<IWorldObject>>();
		WorldObject.SetNetworkBehaviour((NetworkBehaviour)networkBehaviour);
		networkBehaviour.SetWorldObject(WorldObject);
	}
}