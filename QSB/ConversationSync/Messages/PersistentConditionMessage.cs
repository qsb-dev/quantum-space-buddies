using QSB.Messaging;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(_conditionName);
			writer.Write(_conditionState);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			_conditionName = reader.ReadString();
			_conditionState = reader.ReadBoolean();
		}

		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;

		public override void OnReceiveRemote()
		{
			if (QSBCore.IsHost)
			{
				QSBWorldSync.SetPersistentCondition(_conditionName, _conditionState);
			}

			DebugLog.DebugWrite($"Got persistentcondition {_conditionName} value:{_conditionState}");

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
