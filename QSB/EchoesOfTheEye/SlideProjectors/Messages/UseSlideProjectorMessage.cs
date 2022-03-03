using QSB.EchoesOfTheEye.SlideProjectors.WorldObjects;
using QSB.Messaging;
using QSB.Player;

namespace QSB.EchoesOfTheEye.SlideProjectors.Messages;

public class UseSlideProjectorMessage : QSBWorldObjectMessage<QSBSlideProjector, uint>
{
	public UseSlideProjectorMessage(bool @using) => Data = @using ? QSBPlayerManager.LocalPlayerId : 0;
	public UseSlideProjectorMessage(uint user) => Data = user;
	public override void OnReceiveLocal() => OnReceiveRemote();
	public override void OnReceiveRemote() => WorldObject.SetUser(Data);
}
