using QSB.Messaging;
using QSB.ShipSync.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.ShipSync.Messages;

internal class ModuleDetachMessage : QSBWorldObjectMessage<QSBShipDetachableModule>
{
	public override void OnReceiveRemote()
	{
		WorldObject.AttachedObject.Detach();
	}
}
