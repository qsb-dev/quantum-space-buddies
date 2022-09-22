using QSB.Messaging;

namespace QSB.EchoesOfTheEye.WineCellar.Messages;

internal class WineCellarSwitchMessage : QSBWorldObjectMessage<QSBWineCellarSwitch>
{
	public override void OnReceiveRemote() => WorldObject.AttachedObject.OnPressInteract();
}
