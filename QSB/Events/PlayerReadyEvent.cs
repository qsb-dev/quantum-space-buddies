using QSB.Messaging;
using QSB.Utility;

namespace QSB.Events
{
    public class PlayerReadyEvent : QSBEvent<ToggleMessage>
    {
        public override MessageType Type => MessageType.PlayerReady;

        public override void SetupListener()
        {
            GlobalMessenger<bool>.AddListener(EventNames.QSBPlayerReady, ready => SendEvent(CreateMessage(ready)));
        }

        public override void CloseListener()
        {
            DebugLog.ToConsole("Close listener for ready event");
            GlobalMessenger<bool>.RemoveListener(EventNames.QSBPlayerReady, ready => SendEvent(CreateMessage(ready)));
        }

        private ToggleMessage CreateMessage(bool ready) => new ToggleMessage
        {
            SenderId = LocalPlayerId,
            ToggleValue = ready
        };

        public override void OnServerReceive(ToggleMessage message)
        {
            PlayerRegistry.GetPlayer(message.SenderId).IsReady = message.ToggleValue;
            PlayerState.LocalInstance.Send();
        }
    }
}
