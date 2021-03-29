using QSB.WorldSync;

namespace QSB.CampfireSync.WorldObjects
{
	class QSBCampfire : WorldObject<Campfire>
	{
		public override void Init(Campfire campfire, int id)
		{
			ObjectId = id;
			AttachedObject = campfire;
		}

		public void SetState(Campfire.State newState) 
			=> AttachedObject.SetState(newState);
	}
}