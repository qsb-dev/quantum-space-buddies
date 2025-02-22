using QSB.Messaging;
using QSB.Player;

namespace QSB.HUD.Messages;

public class PlanetMessage : QSBMessage<string>
{
	public PlanetMessage(string planet) : base(planet) { }

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
