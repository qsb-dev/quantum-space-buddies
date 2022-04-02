using QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.Messages;

public class SetEnabledMessage : QSBWorldObjectMessage<QSBAlarmTotem, bool>
{
	public SetEnabledMessage(bool enabled) : base(enabled) { }

	public override void OnReceiveRemote() =>
		WorldObject.SetEnabled(Data);
}
