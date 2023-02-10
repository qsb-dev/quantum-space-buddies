using QSB.Messaging;
using QSB.Player;
using QSB.Utility;

namespace QSB.HUD.Messages;

internal class PlanetMessage : QSBMessage<HUDIcon>
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

		DebugLog.DebugWrite($"{From} now on {Data}");

		from.HUDBox?.UpdateIcon(Data);
	}
}
