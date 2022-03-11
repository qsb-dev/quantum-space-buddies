using QSB.CampfireSync.WorldObjects;
using QSB.Messaging;

namespace QSB.CampfireSync.Messages;

internal class CampfireStateMessage : QSBWorldObjectMessage<QSBCampfire, Campfire.State>
{
	public CampfireStateMessage(Campfire.State state) : base(state) { }

	public override void OnReceiveRemote() => WorldObject.SetState(Data);
}