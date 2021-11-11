using QSB.EchoesOfTheEye.AirlockSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.AirlockSync
{
	internal class AirlockManager : WorldObjectManager
	{
		protected override void RebuildWorldObjects(OWScene scene) => QSBWorldSync.Init<QSBGhostAirlock, GhostAirlock>();
	}
}
