using QSB.Messaging;
using QSB.TransformSync;
using QSB.Utility;

namespace QSB.Events
{
    public abstract class QSBEvent<T> where T : PlayerMessage, new()

    {
        public abstract MessageType Type { get; }

        public abstract void SetupListener();
        public abstract void OnReceive(T message);

        public virtual void OnReceiveLocal(T message)
        {
            OnReceive(message);
        }

        public void SendEvent(T message)
        {
            UnityHelper.Instance.RunWhen(() => PlayerTransformSync.LocalInstance != null, () => Send(message));
        }

        private readonly MessageHandler<T> _eventHandler;

        protected QSBEvent()
        {
            _eventHandler = new MessageHandler<T>(Type);
            _eventHandler.OnClientReceiveMessage += OnClientReceiveMessage;
            _eventHandler.OnServerReceiveMessage += OnServerReceiveMessage;

            SetupListener();
        }

        private void Send(T message)
        {
            _eventHandler.SendToServer(message);
        }

        private void OnServerReceiveMessage(T message)
        {
            _eventHandler.SendToAll(message);
        }

        private void OnClientReceiveMessage(T message)
        {
            if (message.SenderId == PlayerRegistry.LocalPlayer?.NetId)
            {
                OnReceiveLocal(message);
                return;
            }

            OnReceive(message);
        }
    }
}
