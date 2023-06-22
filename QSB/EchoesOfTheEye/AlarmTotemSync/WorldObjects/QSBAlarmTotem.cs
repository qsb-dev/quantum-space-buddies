using QSB.EchoesOfTheEye.AlarmTotemSync.Messages;
using QSB.Messaging;
using QSB.OwnershipSync;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;

public class QSBAlarmTotem : OwnedWorldObject<AlarmTotem>
{
	public override bool CanOwn => AttachedObject.enabled;

	public override void SendInitialState(uint to)
	{
		base.SendInitialState(to);

		this.SendMessage(new SetVisibleMessage(AttachedObject._isPlayerVisible) { To = to });
	}
}
