using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.ConversationSync.Messages;

public class PersistentConditionMessage : QSBMessage<(string Condition, bool State)>
{
	public PersistentConditionMessage(string condition, bool state) : base((condition, state)) { }

	public override void OnReceiveRemote()
	{
		if (QSBCore.IsHost)
		{
			QSBWorldSync.SetPersistentCondition(Data.Condition, Data.State);
		}

		var gameSave = PlayerData._currentGameSave;
		if (gameSave.dictConditions.ContainsKey(Data.Condition))
		{
			gameSave.dictConditions[Data.Condition] = Data.State;
		}
		else
		{
			gameSave.dictConditions.Add(Data.Condition, Data.State);
		}

		if (Data.Condition
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
			QSBWorldSync.SetPersistentCondition(Data.Condition, Data.State);
		}
	}
}