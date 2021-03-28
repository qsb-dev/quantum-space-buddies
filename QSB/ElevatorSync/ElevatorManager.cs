using QSB.ElevatorSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.ElevatorSync
{
	public class ElevatorManager : WorldObjectManager
	{
		protected override void RebuildWorldObjects(OWScene scene)
			=> QSBWorldSync.Init<QSBElevator, Elevator>();
	}
}