using QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.Messages;

public class TotemEnabledMessage : QSBWorldObjectMessage<QSBAlarmTotem, bool>
{
	public TotemEnabledMessage(bool enabled) : base(enabled) { }

	public override void OnReceiveRemote() =>
		WorldObject.SetEnabled(Data);
}
