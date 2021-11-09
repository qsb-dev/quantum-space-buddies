using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.AirlockSync.WorldObjects
{
	internal class QSBGhostAirlock : WorldObject<GhostAirlock>
	{
		public override void Init(GhostAirlock airlock, int id)
		{
			ObjectId = id;
			AttachedObject = airlock;
		}
	}
}
