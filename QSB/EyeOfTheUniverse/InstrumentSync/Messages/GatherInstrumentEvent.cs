using QSB.Events;
using QSB.EyeOfTheUniverse.InstrumentSync.WorldObjects;
using QSB.WorldSync;
using QSB.WorldSync.Events;

namespace QSB.EyeOfTheUniverse.InstrumentSync.Messages
{
	internal class GatherInstrumentEvent : QSBEvent<WorldObjectMessage>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener() => GlobalMessenger<QSBQuantumInstrument>.AddListener(EventNames.QSBGatherInstrument, Handler);
		public override void CloseListener() => GlobalMessenger<QSBQuantumInstrument>.RemoveListener(EventNames.QSBGatherInstrument, Handler);

		private void Handler(QSBQuantumInstrument instrument) => SendEvent(CreateMessage(instrument));

		private BoolWorldObjectMessage CreateMessage(QSBQuantumInstrument instrument) => new()
		{
			AboutId = LocalPlayerId,
			ObjectId = instrument.ObjectId
		};

		public override void OnReceiveRemote(bool isHost, WorldObjectMessage message)
		{
			var qsbObj = QSBWorldSync.GetWorldFromId<QSBQuantumInstrument>(message.ObjectId);
			qsbObj.AttachedObject.Gather();
		}
	}
}
