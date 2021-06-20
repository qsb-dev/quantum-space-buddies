﻿using QSB.Events;
using QSB.ShipSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using QSB.WorldSync.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.ShipSync.Events.Hull
{
	class HullDamagedEvent : QSBEvent<WorldObjectMessage>
	{
		public override EventType Type => EventType.HullDamaged;

		public override void SetupListener() => GlobalMessenger<ShipHull>.AddListener(EventNames.QSBHullDamaged, Handler);
		public override void CloseListener() => GlobalMessenger<ShipHull>.RemoveListener(EventNames.QSBHullDamaged, Handler);

		private void Handler(ShipHull hull) => SendEvent(CreateMessage(hull));

		private WorldObjectMessage CreateMessage(ShipHull hull)
		{
			var worldObject = QSBWorldSync.GetWorldFromUnity<QSBShipHull, ShipHull>(hull);
			return new WorldObjectMessage
			{
				AboutId = LocalPlayerId,
				ObjectId = worldObject.ObjectId
			};
		}

		public override void OnReceiveLocal(bool server, WorldObjectMessage message)
		{
			DebugLog.DebugWrite($"[HULL] {message.ObjectId} OnDamaged.", OWML.Common.MessageType.Warning);
		}

		public override void OnReceiveRemote(bool server, WorldObjectMessage message)
		{
			var worldObject = QSBWorldSync.GetWorldFromId<QSBShipHull>(message.ObjectId);
			worldObject.SetDamaged();
		}
	}
}
