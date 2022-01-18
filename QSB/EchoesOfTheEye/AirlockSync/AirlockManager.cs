using QSB.EchoesOfTheEye.AirlockSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.AirlockSync
{
	internal class AirlockManager : WorldObjectManager
	{
		// is this used in the prisoner sequence in the eye?
		public override WorldObjectType WorldObjectType => WorldObjectType.SolarSystem;

		public override void BuildWorldObjects(OWScene scene) => QSBWorldSync.Init<QSBGhostAirlock, GhostAirlock>();
	}
}
