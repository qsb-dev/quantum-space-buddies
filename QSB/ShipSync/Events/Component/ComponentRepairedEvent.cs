using QSB.Events;
using QSB.ShipSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync.Events;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.ShipSync.Events.Component
{
	class ComponentRepairedEvent : QSBEvent<WorldObjectMessage>
	{
		public override EventType Type => EventType.ComponentRepaired;

		public override void SetupListener() => GlobalMessenger<ShipComponent>.AddListener(EventNames.QSBComponentRepaired, Handler);
		public override void CloseListener() => GlobalMessenger<ShipComponent>.RemoveListener(EventNames.QSBComponentRepaired, Handler);

		private void Handler(ShipComponent hull) => SendEvent(CreateMessage(hull));

		private WorldObjectMessage CreateMessage(ShipComponent hull)
		{
			var worldObject = QSBWorldSync.GetWorldFromUnity<QSBShipComponent, ShipComponent>(hull);
			return new WorldObjectMessage
			{
				AboutId = LocalPlayerId,
				ObjectId = worldObject.ObjectId
			};
		}

		public override void OnReceiveLocal(bool server, WorldObjectMessage message)
		{
			DebugLog.DebugWrite($"[S COMPONENT] {message.ObjectId} OnRepaired.", OWML.Common.MessageType.Warning);
		}

		public override void OnReceiveRemote(bool server, WorldObjectMessage message)
		{
			var worldObject = QSBWorldSync.GetWorldFromId<QSBShipComponent>(message.ObjectId);
			worldObject.SetRepaired();
		}
	}
}
