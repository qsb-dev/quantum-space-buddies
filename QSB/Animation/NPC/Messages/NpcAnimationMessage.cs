using QSB.Animation.NPC.WorldObjects;
using QSB.Messaging;

namespace QSB.Animation.NPC.Messages;

internal class NpcAnimationMessage : QSBWorldObjectMessage<INpcAnimController, bool>
{
	public NpcAnimationMessage(bool start) => Value = start;

	public override void OnReceiveRemote()
	{
		if (Value)
		{
			WorldObject.StartConversation();
		}
		else
		{
			WorldObject.EndConversation();
		}
	}
}