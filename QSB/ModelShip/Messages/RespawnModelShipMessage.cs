using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.ModelShip.Messages;

public class RespawnModelShipMessage : QSBMessage<bool>
{
	public RespawnModelShipMessage(bool playEffects) : base(playEffects) { }

	public override void OnReceiveRemote()
	{
		var flightConsole = QSBWorldSync.GetUnityObject<RemoteFlightConsole>();
		flightConsole.RespawnModelShip(Data);
		if (Data)
		{
			flightConsole._modelShipBody.GetComponent<OWAudioSource>().PlayOneShot(AudioType.TH_RetrieveModelShip);
		}
	}
}
