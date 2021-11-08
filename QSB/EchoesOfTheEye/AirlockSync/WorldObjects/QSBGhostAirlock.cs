using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.EchoesOfTheEye.AirlockSync.WorldObjects
{
	class QSBGhostAirlock : WorldObject<GhostAirlock>
	{
		public override void Init(GhostAirlock airlock, int id)
		{
			ObjectId = id;
			AttachedObject = airlock;
		}
	}
}
