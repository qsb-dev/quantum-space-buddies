using QSB.WorldSync;

namespace QSB.CampfireSync.WorldObjects
{
	public class QSBCampfire : WorldObject<Campfire>
	{
		public override void Init(Campfire campfire, int id)
		{
			ObjectId = id;
			AttachedObject = campfire;
		}

		public void StartRoasting()
			=> AttachedObject.StartRoasting();

		public Campfire.State GetState()
			=> AttachedObject.GetState();

		public void SetState(Campfire.State newState)
			=> AttachedObject.SetState(newState);
	}
}
