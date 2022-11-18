using QSB.AuthoritySync;
using QSB.EchoesOfTheEye.AlarmTotemSync.Messages;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;

/// <summary>
/// TODO: make this not NRE (by not doing enable sync) and then readd it back in
/// </summary>
public class QSBAlarmTotem : AuthWorldObject<AlarmTotem>
{
	public override bool CanOwn => AttachedObject.enabled;

	public override void SendInitialState(uint to)
	{
		base.SendInitialState(to);

		this.SendMessage(new SetVisibleMessage(AttachedObject._isPlayerVisible) { To = to });
	}
}
