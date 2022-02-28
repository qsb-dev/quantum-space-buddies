using Mirror;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.ConversationSync.Messages
{
	internal class PersistentConditionMessage : QSBMessage<string, bool>
	{
		public PersistentConditionMessage(string condition, bool state)
		{
			Value1 = condition;
			Value2 = state;
		}

		public override void OnReceiveRemote()
		{
			if (QSBCore.IsHost)
			{
				QSBWorldSync.SetPersistentCondition(Value1, Value2);
			}

			var gameSave = PlayerData._currentGameSave;
			if (gameSave.dictConditions.ContainsKey(Value1))
			{
				gameSave.dictConditions[Value1] = Value2;
			}
			else
			{
				gameSave.dictConditions.Add(Value1, Value2);
			}

			if (Value1
				is not "LAUNCH_CODES_GIVEN"
			    and not "PLAYER_ENTERED_TIMELOOPCORE"
			    and not "PROBE_ENTERED_TIMELOOPCORE"
			    and not "PLAYER_ENTERED_TIMELOOPCORE_MULTIPLE")
			{
				PlayerData.SaveCurrentGame();
			}
		}

		public override void OnReceiveLocal()
		{
			if (QSBCore.IsHost)
			{
				QSBWorldSync.SetPersistentCondition(Value1, Value2);
			}
		}
	}
}