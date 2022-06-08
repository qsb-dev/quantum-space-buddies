using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.ModelShip.Messages;

internal class RespawnModelShipMessage : QSBMessage<bool>
{
	public RespawnModelShipMessage(bool playEffects) : base(playEffects) { }

	public override void OnReceiveRemote() =>
		QSBPatch.RemoteCall(() => QSBWorldSync.GetUnityObject<RemoteFlightConsole>().RespawnModelShip(Data));
}
