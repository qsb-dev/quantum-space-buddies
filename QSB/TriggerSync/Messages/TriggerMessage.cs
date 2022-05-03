using QSB.Messaging;
using QSB.Player;
using QSB.TriggerSync.WorldObjects;

namespace QSB.TriggerSync.Messages;

public class TriggerMessage : QSBWorldObjectMessage<IQSBTrigger, bool>
{
	public TriggerMessage(bool entered) : base(entered) { }

	public override void OnReceiveLocal() => OnReceiveRemote();

	public override void OnReceiveRemote()
	{
		var player = QSBPlayerManager.GetPlayer(From);
		if (Data)
		{
			WorldObject.Enter(player);
		}
		else
		{
			WorldObject.Exit(player);
		}
	}
}