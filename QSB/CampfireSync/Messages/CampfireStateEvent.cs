using QSB.CampfireSync.WorldObjects;
using QSB.Events;
using QSB.WorldSync;
using QSB.WorldSync.Events;

namespace QSB.CampfireSync.Messages
{
	internal class CampfireStateEvent : QSBEvent<EnumWorldObjectMessage<Campfire.State>>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener() => GlobalMessenger<int, Campfire.State>.AddListener(EventNames.QSBCampfireState, Handler);
		public override void CloseListener() => GlobalMessenger<int, Campfire.State>.RemoveListener(EventNames.QSBCampfireState, Handler);

		private void Handler(int objId, Campfire.State state) => SendEvent(CreateMessage(objId, state));

		private EnumWorldObjectMessage<Campfire.State> CreateMessage(int objId, Campfire.State state) => new()
		{
			AboutId = LocalPlayerId,
			ObjectId = objId,
			EnumValue = state
		};

		public override void OnReceiveRemote(bool server, EnumWorldObjectMessage<Campfire.State> message)
		{
			var campfireObj = QSBWorldSync.GetWorldFromId<QSBCampfire>(message.ObjectId);
			campfireObj.SetState(message.EnumValue);
		}
	}
}
