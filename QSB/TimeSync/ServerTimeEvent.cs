using QSB.Events;
using QSB.Messaging;

namespace QSB.TimeSync
{
    public class ServerTimeEvent : QSBEvent<ServerTimeMessage>
    {
        public override MessageType Type => MessageType.ServerTime;

        public override void SetupListener()
        {
            GlobalMessenger<float, int>.AddListener(EventNames.QSBServerTime, (time, count) => SendEvent(CreateMessage(time, count)));
        }

        public override void CloseListener()
        {
            GlobalMessenger<float, int>.RemoveListener(EventNames.QSBServerTime, (time, count) => SendEvent(CreateMessage(time, count)));
        }

        private ServerTimeMessage CreateMessage(float time, int count) => new ServerTimeMessage
        {
            SenderId = PlayerRegistry.LocalPlayer.NetId,
            ServerTime = time,
            LoopCount = count
        };

        public override void OnReceiveRemote(ServerTimeMessage message)
        {
            WakeUpSync.LocalInstance.OnClientReceiveMessage(message);
        }
    }
}
