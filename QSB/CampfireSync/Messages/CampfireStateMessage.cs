using QSB.CampfireSync.WorldObjects;
using QSB.Messaging;

namespace QSB.CampfireSync.Messages
{
	internal class CampfireStateMessage : QSBEnumWorldObjectMessage<QSBCampfire, Campfire.State>
	{
		public CampfireStateMessage(Campfire.State state) => Value = state;

		public CampfireStateMessage() { }

		public override void OnReceiveRemote() => WorldObject.SetState(Value);
	}
}
