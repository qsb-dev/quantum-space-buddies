using Mirror;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.ConversationSync.Messages
{
	internal class PersistentConditionMessage : QSBMessage
	{
		private string _conditionName;
		private bool _conditionState;

		public PersistentConditionMessage(string condition, bool state)
		{
			_conditionName = condition;
			_conditionState = state;
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(_conditionName);
			writer.Write(_conditionState);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			_conditionName = reader.ReadString();
			_conditionState = reader.ReadBool();
		}

		public override void OnReceiveRemote()
		{
			if (QSBCore.IsHost)
			{
				QSBWorldSync.SetPersistentCondition(_conditionName, _conditionState);
			}

			var gameSave = PlayerData._currentGameSave;
			if (gameSave.dictConditions.ContainsKey(_conditionName))
			{
				gameSave.dictConditions[_conditionName] = _conditionState;
			}
			else
			{
				gameSave.dictConditions.Add(_conditionName, _conditionState);
			}

			if (_conditionName
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
				QSBWorldSync.SetPersistentCondition(_conditionName, _conditionState);
			}
		}
	}
}
