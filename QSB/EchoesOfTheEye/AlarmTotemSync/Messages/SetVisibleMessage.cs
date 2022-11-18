using QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.Messages;

public class SetVisibleMessage : QSBWorldObjectMessage<QSBAlarmTotem, bool>
{
	public SetVisibleMessage(bool visible) : base(visible) { }
	public override void OnReceiveRemote() => WorldObject.SetVisible(true);
}
