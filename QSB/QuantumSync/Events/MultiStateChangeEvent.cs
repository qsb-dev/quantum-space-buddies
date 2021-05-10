using OWML.Common;
using QSB.Events;
using QSB.QuantumSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.QuantumSync.Events
{
	public class MultiStateChangeEvent : QSBEvent<MultiStateChangeMessage>
	{
		public override EventType Type => EventType.MultiStateChange;

		public override void SetupListener() => GlobalMessenger<int, int>.AddListener(EventNames.QSBMultiStateChange, Handler);
		public override void CloseListener() => GlobalMessenger<int, int>.RemoveListener(EventNames.QSBMultiStateChange, Handler);

		private void Handler(int objid, int stateIndex) => SendEvent(CreateMessage(objid, stateIndex));

		private MultiStateChangeMessage CreateMessage(int objid, int stateIndex) => new MultiStateChangeMessage
		{
			AboutId = LocalPlayerId,
			ObjectId = objid,
			StateIndex = stateIndex
		};

		public override void OnReceiveLocal(bool server, MultiStateChangeMessage message)
		{
			if (!QSBCore.DebugMode)
			{
				return;
			}
			var qsbObj = QSBWorldSync.GetWorldFromId<QSBMultiStateQuantumObject>(message.ObjectId);
			qsbObj.DebugBoxText.text = message.StateIndex.ToString();
		}

		public override void OnReceiveRemote(bool server, MultiStateChangeMessage message)
		{
			if (!QSBCore.WorldObjectsReady)
			{
				return;
			}
			var qsbObj = QSBWorldSync.GetWorldFromId<QSBMultiStateQuantumObject>(message.ObjectId);
			if (qsbObj.ControllingPlayer != message.FromId)
			{
				DebugLog.ToConsole($"Error - Got MultiStateChangeEvent for {qsbObj.Name} from {message.FromId}, but it's currently controlled by {qsbObj.ControllingPlayer}!", MessageType.Error);
				return;
			}
			qsbObj.ChangeState(message.StateIndex);
		}
	}
}