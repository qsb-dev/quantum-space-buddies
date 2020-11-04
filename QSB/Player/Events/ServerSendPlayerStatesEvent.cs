using QSB.EventsCore;

namespace QSB.Player.Events
{
    public class ServerSendPlayerStatesEvent : QSBEvent<PlayerStateMessage>
    {
        public override EventType Type => EventType.PlayerState;

        public override void SetupListener() => GlobalMessenger.AddListener(EventNames.QSBServerSendPlayerStates, Handler);

        public override void CloseListener() => GlobalMessenger.RemoveListener(EventNames.QSBServerSendPlayerStates, Handler);

        private void Handler()
        {
            foreach (var player in QSBPlayerManager.PlayerList)
            {
                SendEvent(CreateMessage(player));
            }
        }

        private PlayerStateMessage CreateMessage(PlayerInfo player) => new PlayerStateMessage
        {
            AboutId = player.PlayerId,
            PlayerName = player.Name,
            PlayerReady = player.IsReady,
            PlayerState = player.State
        };

        public override void OnReceiveRemote(PlayerStateMessage message)
        {
            QSB.Helper.Events.Unity.RunWhen(
                () => QSBPlayerManager.GetSyncObject<TransformSync.TransformSync>(message.AboutId) != null,
                () => QSBPlayerManager.HandleFullStateMessage(message));
        }
    }
}
