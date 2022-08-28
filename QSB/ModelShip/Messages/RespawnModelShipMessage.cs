using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.ModelShip.Messages;

internal class RespawnModelShipMessage : QSBMessage<bool>
{
	public RespawnModelShipMessage(bool playEffects) : base(playEffects) { }

	public override void OnReceiveRemote()
	{
		var flightConsole = QSBWorldSync.GetUnityObject<RemoteFlightConsole>();
		QSBPatch.RemoteCall(() => flightConsole.RespawnModelShip(Data));
		if (Data) flightConsole._modelShipBody.GetComponent<OWAudioSource>().PlayOneShot(AudioType.TH_RetrieveModelShip);
	}
}
