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
	public LinkMessage(IWorldObject worldObject, INetworkBehaviour networkBehaviour) :
		base((worldObject.ObjectId, networkBehaviour.netId)) { }

	public override void OnReceiveRemote()
	{
		var worldObject = Data.ObjectId.GetWorldObject<ILinkedWorldObject<INetworkBehaviour>>();
		var networkBehaviour = NetworkClient.spawned[Data.NetId].GetComponent<ILinkedNetworkBehaviour<IWorldObject>>();

		worldObject.LinkTo(networkBehaviour);
		networkBehaviour.LinkTo(worldObject);
	}
}
