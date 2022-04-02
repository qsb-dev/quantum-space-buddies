using QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;
using QSB.Messaging;
using QSB.Player;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.Messages;

public class SetVisibleMessage : QSBWorldObjectMessage<QSBAlarmTotem, (uint playerId, bool visible)>
{
	public SetVisibleMessage(bool visible) : base((QSBPlayerManager.LocalPlayerId, visible)) { }
	public SetVisibleMessage(uint playerId, bool visible) : base((playerId, visible)) { }
	public override void OnReceiveLocal() => OnReceiveRemote();
	public override void OnReceiveRemote() => WorldObject.SetVisible(Data.playerId, Data.visible);
}
