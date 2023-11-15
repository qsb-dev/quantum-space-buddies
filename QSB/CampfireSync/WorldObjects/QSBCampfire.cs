using QSB.CampfireSync.Messages;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.CampfireSync.WorldObjects;

public class QSBCampfire : WorldObject<Campfire>
{
	public void StartRoasting()
		=> AttachedObject.StartRoasting();

	public Campfire.State GetState()
		=> AttachedObject.GetState();

	public void SetState(Campfire.State newState)
		=> AttachedObject.SetState(newState);
}