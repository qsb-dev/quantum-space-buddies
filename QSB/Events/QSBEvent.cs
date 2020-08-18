using QSB.Messaging;
using QSB.TransformSync;
using QSB.Utility;

namespace QSB.Events
{
    /// <summary>
    /// Abstract class that handles all event code.
    /// </summary>
    /// <typeparam name="T">The message type to use.</typeparam>
    public abstract class QSBEvent<T> : IQSBEvent where T : PlayerMessage, new()
    {
        public abstract MessageType Type { get; }
        public uint LocalPlayerId => PlayerRegistry.LocalPlayerId;
        private readonly MessageHandler<T> _eventHandler;

        protected QSBEvent()
        {
            _eventHandler = new MessageHandler<T>(Type);
            _eventHandler.OnClientReceiveMessage += OnClientReceive;
            _eventHandler.OnServerReceiveMessage += OnServerReceive;
        }

        /// <summary>
        /// Called to set up the activators for the event.
        /// </summary>
        public abstract void SetupListener();

        /// <summary>
        /// Called to remove all set up activators.
        /// </summary>
        public abstract void CloseListener();

        /// <summary>
        /// Called on every client that didn't send the event.
        /// </summary>
        /// <param name="message"></param>
        public virtual void OnReceiveRemote(T message)
        {
        }

        /// <summary>
        /// Called on the client that sent the event.
        /// </summary>
        /// <param name="message"></param>
        public virtual void OnReceiveLocal(T message)
        {
        }

        /// <summary>
        /// Called on the server.
        /// </summary>
        /// <param name="message"></param>
        public virtual void OnServerReceive(T message)
        {
            _eventHandler.SendToAll(message);
        }

        public void SendEvent(T message)
        {
            message.FromId = PlayerRegistry.LocalPlayerId;
            UnityHelper.Instance.RunWhen(() => PlayerTransformSync.LocalInstance != null, () => Send(message));
        }

        private void Send(T message)
        {
            _eventHandler.SendToServer(message);
        }

        private void OnClientReceive(T message)
        {
            if (message.FromId == PlayerRegistry.LocalPlayerId ||
                PlayerRegistry.IsBelongingToLocalPlayer(message.AboutId))
            {
                OnReceiveLocal(message);
                return;
            }

            OnReceiveRemote(message);
        }
    }
}
