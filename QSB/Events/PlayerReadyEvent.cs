using QSB.Messaging;
using QSB.Utility;

namespace QSB.Events
{
    class PlayerReadyEvent : QSBEvent<ToggleMessage>
    {
        public override MessageType Type => MessageType.PlayerReady;

        public override void SetupListener()
        {
            GlobalMessenger<bool>.AddListener("QSBPlayerReady", ready => SendEvent(
                new ToggleMessage { 
                    SenderId = LocalPlayerId, 
                    ToggleValue = ready 
                }));
        }

        public override void OnServerReceive(ToggleMessage message)
        {
            DebugLog.ToConsole($"Receieved ready message from {message.SenderId}");
            PlayerRegistry.GetPlayer(message.SenderId).IsReady = message.ToggleValue;
            PlayerState.LocalInstance.Send();
        }
    }
}
