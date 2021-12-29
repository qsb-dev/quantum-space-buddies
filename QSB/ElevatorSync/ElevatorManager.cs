using QSB.ElevatorSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.ElevatorSync
{
	public class ElevatorManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.SolarSystem;

		protected override void RebuildWorldObjects(OWScene scene)
			=> QSBWorldSync.Init<QSBElevator, Elevator>();
	}
}