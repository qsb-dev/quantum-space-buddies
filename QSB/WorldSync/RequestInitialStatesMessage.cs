using QSB.ConversationSync.Messages;
using QSB.LogSync.Messages;
using QSB.Messaging;
using QSB.Utility;

namespace QSB.WorldSync
{
	/// <summary>
	/// sent to the host to get initial object states.
	/// <para/>
	/// world objects will be ready on both sides at this point
	/// </summary>
	public class RequestInitialStatesMessage : QSBMessage
	{
		public RequestInitialStatesMessage() => To = 0;

		public override void OnReceiveRemote() =>
			Delay.RunWhen(() => QSBWorldSync.AllObjectsReady,
				() => SendInitialStates(From));

		private static void SendInitialStates(uint to)
		{
			DebugLog.DebugWrite($"sending initial states to {to}");

			QSBWorldSync.DialogueConditions.ForEach(condition
				=> new DialogueConditionMessage(condition.Key, condition.Value) { To = to }.Send());

			QSBWorldSync.ShipLogFacts.ForEach(fact
				=> new RevealFactMessage(fact.Id, fact.SaveGame, false) { To = to }.Send());

			var target = to.GetNetworkConnection();
			foreach (var qsbNetworkBehaviour in QSBWorldSync.GetUnityObjects<QSBNetworkBehaviour>())
			{
				qsbNetworkBehaviour.SendInitialState(target);
			}

			foreach (var worldObject in QSBWorldSync.GetWorldObjects())
			{
				worldObject.SendInitialState(to);
			}
		}
	}
}