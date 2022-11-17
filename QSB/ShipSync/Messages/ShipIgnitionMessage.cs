using QSB.Messaging;
using QSB.Player;
using QSB.Player.TransformSync;
using static QSB.ShipSync.Messages.ShipIgnitionMessage;

namespace QSB.ShipSync.Messages;

internal class ShipIgnitionMessage : QSBMessage<ShipIgnitionType>
{
    public enum ShipIgnitionType
    {
        START_IGNITION,
        COMPLETE_IGNITION,
        CANCEL_IGNITION
    }

    static ShipIgnitionMessage()
    {
        GlobalMessenger.AddListener(OWEvents.StartShipIgnition, () => Handler(ShipIgnitionType.START_IGNITION));
        GlobalMessenger.AddListener(OWEvents.CompleteShipIgnition, () => Handler(ShipIgnitionType.COMPLETE_IGNITION));
        GlobalMessenger.AddListener(OWEvents.CancelShipIgnition, () => Handler(ShipIgnitionType.CANCEL_IGNITION));
    }

    public ShipIgnitionMessage(ShipIgnitionType data) : base(data) { }

    private static void Handler(ShipIgnitionType type)
    {
        if (PlayerTransformSync.LocalInstance && QSBPlayerManager.LocalPlayer.FlyingShip)
        {
            new ShipIgnitionMessage(type).Send();
        }
    }

    public override void OnReceiveRemote()
    {
		switch (Data)
		{
			case ShipIgnitionType.START_IGNITION:
				GlobalMessenger.FireEvent(OWEvents.StartShipIgnition);
				break;
			case ShipIgnitionType.COMPLETE_IGNITION:
				GlobalMessenger.FireEvent(OWEvents.CompleteShipIgnition);
				break;
			case ShipIgnitionType.CANCEL_IGNITION:
				GlobalMessenger.FireEvent(OWEvents.CancelShipIgnition);
				break;
		}
	}
}
