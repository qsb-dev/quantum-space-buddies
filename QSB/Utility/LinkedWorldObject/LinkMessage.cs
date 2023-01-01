using Mirror;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.Utility.LinkedWorldObject;

/// <summary>
/// sent from the host to a non-host
/// telling a world object and network behaviour to link
/// </summary>
public class LinkMessage : QSBMessage<(int ObjectId, uint NetId)>
{
	public LinkMessage(IWorldObject worldObject, NetworkBehaviour networkBehaviour)
		: base((worldObject.ObjectId, networkBehaviour.netId))
	{ }

	public override void OnReceiveRemote()
	{
		var worldObject = Data.ObjectId.GetWorldObject<ILinkedWorldObject<NetworkBehaviour>>();
		var identity = NetworkClient.spawned[Data.NetId];
		var networkBehaviour = identity.GetComponent<ILinkedNetworkBehaviour>();

		worldObject.LinkTo(networkBehaviour);
	}
}
