using QSB.WorldSync;
using System.Reflection;

namespace QSB.CampfireSync.WorldObjects
{
	public class QSBCampfire : WorldObject<Campfire>
	{
		public void StartRoasting()
			=> AttachedObject
				.GetType()
				.GetMethod("StartRoasting", BindingFlags.NonPublic | BindingFlags.Instance)
				.Invoke(AttachedObject, null);

		public Campfire.State GetState()
			=> AttachedObject.GetState();

		public void SetState(Campfire.State newState)
			=> AttachedObject.SetState(newState);
	}
}