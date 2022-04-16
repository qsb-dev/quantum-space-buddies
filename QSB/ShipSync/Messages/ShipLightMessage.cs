using QSB.Messaging;
using QSB.ShipSync.WorldObjects;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.ShipSync.Messages;

internal class ShipLightMessage : QSBWorldObjectMessage<QSBShipLight, bool>
{
	public ShipLightMessage(bool on) : base(on) { }

	public override void OnReceiveRemote()
	{
		WorldObject.SetOn(Data);
	}
}
