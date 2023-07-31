using QSB.Messaging;
using QSB.Player;
using QSB.Utility;

namespace QSB.HUD.Messages;

public class PlanetMessage : QSBMessage<HUDIcon>
{
	public PlanetMessage(HUDIcon icon) : base(icon) { }

	public override void OnReceiveLocal() => OnReceiveRemote();

	public override void OnReceiveRemote()
	{
		var from = QSBPlayerManager.GetPlayer(From);

		if (from == default)
		{
			return;
		}

		from.HUDBox?.UpdateIcon(Data);
	}
}
