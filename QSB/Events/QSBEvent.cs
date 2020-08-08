using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.Events
{
    public abstract class QSBEvent
    {
        public abstract EventType Type { get; }

        public abstract void SetupListener();
        public abstract void OnReceive(uint sender, object[] data);
        public abstract void OnReceiveLocal(object[] data);
    }
}
