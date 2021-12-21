using QSB.EchoesOfTheEye.AirlockSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.AirlockSync
{
	internal class AirlockManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.SolarSystem;

		protected override void RebuildWorldObjects(OWScene scene) => QSBWorldSync.Init<QSBGhostAirlock, GhostAirlock>();
	}
}
