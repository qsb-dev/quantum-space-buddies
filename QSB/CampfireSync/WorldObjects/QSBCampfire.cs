using QSB.WorldSync;
using System.Reflection;

namespace QSB.CampfireSync.WorldObjects
{
	internal class QSBCampfire : WorldObject<Campfire>
	{
		public override void Init(Campfire campfire, int id)
		{
			ObjectId = id;
			AttachedObject = campfire;
		}

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