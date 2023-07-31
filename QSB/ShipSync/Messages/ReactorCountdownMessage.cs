using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.ShipSync.Messages;

public class ReactorCountdownMessage : QSBMessage<float>
{
	public ReactorCountdownMessage(float countdown) : base(countdown) { }

	public override void OnReceiveRemote()
	{
		var reactor = QSBWorldSync.GetUnityObject<ShipReactorComponent>();
		reactor._criticalCountdown = Data;
		reactor._criticalTimer = Data;
		reactor.enabled = true;
	}
}
