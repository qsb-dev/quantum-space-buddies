using QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.Messages;

public class LocallyVisibleMessage : QSBWorldObjectMessage<QSBAlarmTotem, bool>
{
	public LocallyVisibleMessage(bool visible) : base(visible) { }
	public override void OnReceiveLocal() => OnReceiveRemote();
	public override void OnReceiveRemote() => WorldObject.SetLocallyVisible(From, Data);
}
