using Mirror;
using QSB.Messaging;
using QSB.WorldSync;
using System.Linq;

namespace QSB.Utility.LinkedWorldObject;

/// <summary>
/// sent from the host to a non-host
/// telling a world object and network behaviour to link
/// </summary>
public class LinkMessage : QSBMessage<(int ObjectId, uint NetId)>
{
	public LinkMessage(IWorldObject worldObject, NetworkBehaviour networkBehaviour) :
		base((worldObject.ObjectId, networkBehaviour.netId)) { }

	public override void OnReceiveRemote()
	{
		var worldObject = Data.ObjectId.GetWorldObject<ILinkedWorldObject<NetworkBehaviour>>();
		var networkIdentity = NetworkClient.spawned[Data.NetId];
		var networkBehaviour = networkIdentity.NetworkBehaviours.OfType<ILinkedNetworkBehaviour<IWorldObject>>().First();

		worldObject.LinkTo((NetworkBehaviour)networkBehaviour);
		networkBehaviour.LinkTo(worldObject);
	}
}
