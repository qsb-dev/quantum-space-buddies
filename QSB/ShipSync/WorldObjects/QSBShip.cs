using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.ShipSync.WorldObjects
{
	public class QSBShip : WorldObject<ShipBody>
	{
		public override void Init(ShipBody ship, int id)
		{
			ObjectId = id;
			AttachedObject = ship;
		}
	}
}
