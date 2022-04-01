using QSB.EchoesOfTheEye.AlarmTotemSync.Messages;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;

public class QSBAlarmTotem : WorldObject<AlarmTotem>
{
	public override void SendInitialState(uint to)
	{
		this.SendMessage(new SetFaceOpenMessage(AttachedObject._isFaceOpen) { To = to });
	}
}
