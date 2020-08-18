using QSB.Messaging;
using QSB.Utility;

namespace QSB.Events
{
    public class PlayerReadyEvent : QSBEvent<ToggleMessage>
    {
        public override MessageType Type => MessageType.PlayerReady;

        public override void SetupListener()
        {
            GlobalMessenger<bool>.AddListener(EventNames.QSBPlayerReady, Handler);
        }

        public override void CloseListener()
        {
            GlobalMessenger<bool>.RemoveListener(EventNames.QSBPlayerReady, Handler);
        }

        private void Handler(bool ready) => SendEvent(CreateMessage(ready));

        private ToggleMessage CreateMessage(bool ready) => new ToggleMessage
        {
            AboutId = LocalPlayerId,
            ToggleValue = ready
        };

        public override void OnServerReceive(ToggleMessage message)
        {
            DebugLog.ToConsole("Get ready event for player " + message.AboutId);
            PlayerRegistry.GetPlayer(message.AboutId).IsReady = message.ToggleValue;
            PlayerState.LocalInstance.Send();
        }
    }
}
