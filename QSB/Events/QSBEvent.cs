using QSB.Messaging;

namespace QSB.Events
{
    public abstract class QSBEvent
    {
        public abstract EventType Type { get; }

        public abstract void SetupListener();
        public abstract void OnReceive(uint sender, object[] data);
        public virtual void OnReceiveLocal(object[] data) { }
    }
}
