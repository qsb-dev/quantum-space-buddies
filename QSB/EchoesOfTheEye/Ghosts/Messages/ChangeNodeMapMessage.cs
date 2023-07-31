using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;

public class ChangeNodeMapMessage : QSBWorldObjectMessage<QSBGhostController, int>
{
	public ChangeNodeMapMessage(int nodeMapIndex) : base(nodeMapIndex) { }

	public override void OnReceiveRemote()
	{
		var nodeMap = Data.GetWorldObject<QSBGhostNodeMap>().AttachedObject;

		WorldObject.AttachedObject._nodeMap = nodeMap;
		WorldObject.AttachedObject.transform.parent = nodeMap.transform;
		WorldObject.AttachedObject._nodeRoot = nodeMap.transform;
		WorldObject.AttachedObject.OnNodeMapChanged.Invoke();
	}
}
