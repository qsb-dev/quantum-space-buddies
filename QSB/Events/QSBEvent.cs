using QSB.Messaging;
using QSB.TransformSync;
using QSB.Utility;

namespace QSB.Events
{
    public abstract class QSBEvent<T> where T : PlayerMessage, new()

    {
        public abstract MessageType Type { get; }

        public uint LocalPlayerId => PlayerRegistry.LocalPlayer.NetId;

        public abstract void SetupListener();
        public virtual void OnReceiveRemote(T message)
        {

        }

        public virtual void OnReceiveLocal(T message)
        {
            OnReceiveRemote(message);
        }

        public void SendEvent(T message)
        {
            UnityHelper.Instance.RunWhen(() => PlayerTransformSync.LocalInstance != null, () => Send(message));
        }

        private readonly MessageHandler<T> _eventHandler;

        protected QSBEvent()
        {
            _eventHandler = new MessageHandler<T>(Type);
            _eventHandler.OnClientReceiveMessage += OnClientReceive;
            _eventHandler.OnServerReceiveMessage += OnServerReceive;

            SetupListener();
        }

        private void Send(T message)
        {
            _eventHandler.SendToServer(message);
        }

        public virtual void OnServerReceive(T message)
        {
            _eventHandler.SendToAll(message);
        }

        private void OnClientReceive(T message)
        {
            if (message.SenderId == PlayerTransformSync.LocalInstance?.netId.Value)
            {
                OnReceiveLocal(message);
                return;
            }

            OnReceiveRemote(message);
        }
    }
}
