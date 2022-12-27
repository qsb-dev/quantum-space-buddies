using QSB.EchoesOfTheEye.WineCellar.Messages;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.WineCellar.WorldObjects;

internal class QSBWineCellarSwitch : WorldObject<WineCellarSwitch>
{
	public override void SendInitialState(uint to)
	{
		if (AttachedObject.enabled)
		{
			this.SendMessage(new WineCellarSwitchMessage { To = to });
		}
	}
}
