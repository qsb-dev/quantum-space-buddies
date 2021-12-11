using QSB.CampfireSync.WorldObjects;
using QSB.Messaging;

namespace QSB.CampfireSync.Events
{
	public class CampfireStateMessage : QSBEnumWorldObjectMessage<QSBCampfire, Campfire.State>
	{
		public override void OnReceiveRemote() => WorldObject.SetState(Value);
	}
}
