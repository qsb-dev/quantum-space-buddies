using QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.Messages;

public class TotemVisibleMessage : QSBWorldObjectMessage<QSBAlarmTotem, bool>
{
	public TotemVisibleMessage(bool visible) : base(visible) { }
	public override void OnReceiveLocal() => OnReceiveRemote();
	public override void OnReceiveRemote() => WorldObject.SetVisible(From, Data);
}
