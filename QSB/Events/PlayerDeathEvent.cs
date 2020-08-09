using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QSB.Messaging;

namespace QSB.Events
{
    class PlayerDeathEvent : QSBEvent<PlayerDeathMessage>
    {
        public override MessageType Type => MessageType.PlayerDeath;
    }
}
