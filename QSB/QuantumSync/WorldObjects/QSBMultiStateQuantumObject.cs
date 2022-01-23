using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QSB.QuantumSync.WorldObjects
{
	internal class QSBMultiStateQuantumObject : QSBQuantumObject<MultiStateQuantumObject>
	{
		public List<QSBQuantumState> QuantumStates { get; private set; }
		public int CurrentState => AttachedObject._stateIndex;

		public override void Init()
		{
			base.Init();

			StartDelayedReady();
			QSBCore.UnityEvents.RunWhen(() => QSBWorldSync.AllObjectsAdded, () =>
			{
				FinishDelayedReady();

				QuantumStates = AttachedObject._states.Select(QSBWorldSync.GetWorldObject<QSBQuantumState>).ToList();

				if (QuantumStates.Any(x => x == null))
				{
					DebugLog.ToConsole($"Error - {AttachedObject.name} has one or more null QSBQuantumStates assigned!", OWML.Common.MessageType.Error);
				}
			});
		}

		public override string ReturnLabel()
			=> $"{ToString()}{Environment.NewLine}StateIndex:{AttachedObject._stateIndex}";

		public void ChangeState(int newStateIndex)
		{
			if (CurrentState != -1)
			{
				QuantumStates[CurrentState].SetVisible(false);
			}

			QuantumStates[newStateIndex].SetVisible(true);
			AttachedObject._stateIndex = newStateIndex;
		}
	}
}
