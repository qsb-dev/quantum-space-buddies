using QSB.Messaging;
using QSB.Utility;

namespace QSB.ShipSync.Messages;

public class LandingCameraMessage : QSBMessage<bool>
{
	public LandingCameraMessage(bool on) : base(on) { }

	public override void OnReceiveRemote() 
	{
		if (From != ShipManager.Instance.CurrentFlyer)
		{
			DebugLog.ToConsole($"Warning - Received LandingCameraMessage from someone who isn't flying the ship!", OWML.Common.MessageType.Warning);
		}

		ShipManager.Instance.UpdateLandingCamera(Data);
	}
}
