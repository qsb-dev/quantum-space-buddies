using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.Utility;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;

public class ChangeActionMessage : QSBWorldObjectMessage<QSBGhostBrain, GhostAction.Name>
{
	public ChangeActionMessage(GhostAction.Name name) : base(name) { }

	public override void OnReceiveRemote()
	{
		if (QSBCore.IsHost)
		{
			DebugLog.ToConsole("Error - Received ChangeActionMessage on host. Something has gone horribly wrong!", OWML.Common.MessageType.Error);
			return;
		}

		DebugLog.DebugWrite($"{WorldObject.AttachedObject._name} Change action to {Data}");
		WorldObject.ChangeAction(WorldObject.GetAction(Data), true);
	}
}
