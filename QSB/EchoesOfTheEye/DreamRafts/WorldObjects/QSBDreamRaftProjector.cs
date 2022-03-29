using QSB.EchoesOfTheEye.DreamRafts.Messages;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.DreamRafts.WorldObjects;

public class QSBDreamRaftProjector : WorldObject<DreamRaftProjector>
{
	public override void SendInitialState(uint to)
	{
		if (AttachedObject._lit)
		{
			this.SendMessage(new SpawnRaftMessage { To = to });
		}
	}
}
