using QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.Messages;

public class BellHitMessage : QSBWorldObjectMessage<QSBAlarmBell, float>
{
	public BellHitMessage(float volume) : base(volume) { }

	public override void OnReceiveRemote() =>
		WorldObject.AttachedObject._oneShotSource.PlayOneShot(AudioType.AlarmChime_RW, Data);
}
