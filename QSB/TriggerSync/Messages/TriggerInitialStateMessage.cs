using QSB.Messaging;
using QSB.Player;
using QSB.TriggerSync.WorldObjects;
using System.Collections.Generic;
using System.Linq;

namespace QSB.TriggerSync.Messages;

/// <summary>
/// always sent by host
/// </summary>
public class TriggerInitialStateMessage : QSBWorldObjectMessage<IQSBTrigger, uint[]>
{
	public TriggerInitialStateMessage(IEnumerable<PlayerInfo> occupants) :
		base(occupants.Select(x => x.PlayerId).ToArray())
	{ }

	public override void OnReceiveRemote()
	{
		var serverOccupants = Data.Select(QSBPlayerManager.GetPlayer).ToList();
		foreach (var added in serverOccupants.Except(WorldObject.Occupants).ToList())
		{
			WorldObject.Enter(added);
		}

		foreach (var removed in WorldObject.Occupants.Except(serverOccupants).ToList())
		{
			WorldObject.Exit(removed);
		}
	}
}