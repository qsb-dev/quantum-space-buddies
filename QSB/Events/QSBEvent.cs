using QSB.Messaging;
using QSB.TransformSync;
using QSB.Utility;

namespace QSB.Events
{
    public abstract class QSBEvent
    {
        public abstract MessageType Type { get; }

        public abstract void SetupListener();
        public abstract void OnReceive(uint sender, object[] data);
        public virtual void OnReceiveLocal(object[] data) { }

        public void SendEvent(uint sender, params object[] data)
        {
            var message = new EventMessage
            {
                SenderId = sender,
                EventType = (int)Type,
                Data = data
            };
            Send(message);
        }

        private readonly MessageHandler<EventMessage> _eventHandler;

        protected QSBEvent()
        {
            _eventHandler = new MessageHandler<EventMessage>(Type);
            _eventHandler.OnClientReceiveMessage += OnClientReceiveMessage;
            _eventHandler.OnServerReceiveMessage += OnServerReceiveMessage;

            SetupListener();
        }

        public void Send(EventMessage message)
        {
            UnityHelper.Instance.RunWhen(() => PlayerTransformSync.LocalInstance != null, () =>
            {
                _eventHandler.SendToServer(message);
            });
        }

        private void OnServerReceiveMessage(EventMessage message)
        {
            _eventHandler.SendToAll(message);
        }

        private void OnClientReceiveMessage(EventMessage message)
        {
            if (message.SenderId == PlayerRegistry.LocalPlayer?.NetId)
            {
                return;
            }
            OnReceive(message.SenderId, message.Data);
        }
    }
}
