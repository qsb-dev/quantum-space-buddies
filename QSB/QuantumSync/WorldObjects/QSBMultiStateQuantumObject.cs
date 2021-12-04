using System.Collections.Generic;
using System.Linq;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine.UI;

namespace QSB.QuantumSync.WorldObjects
{
	internal class QSBMultiStateQuantumObject : QSBQuantumObject<MultiStateQuantumObject>
	{
		public List<QSBQuantumState> QuantumStates { get; private set; }
		public Text DebugBoxText;
		public int CurrentState => AttachedObject._stateIndex;

		public override void OnRemoval()
		{
			base.OnRemoval();
			if (DebugBoxText != null)
			{
				UnityEngine.Object.Destroy(DebugBoxText.gameObject);
			}
		}

		public override void Init(MultiStateQuantumObject attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;

			if (QSBCore.ShowQuantumDebugBoxes)
			{
				DebugBoxText = DebugBoxManager.CreateBox(AttachedObject.transform, 0, $"Multistate\r\nid:{id}\r\nstate:{CurrentState}").GetComponent<Text>();
			}

			base.Init(attachedObject, id);

			StartDelayedReady();
			QSBCore.UnityEvents.RunWhen(() => WorldObjectManager.AllObjectsAdded, () =>
			{
				FinishDelayedReady();

				QuantumStates = AttachedObject._states.Select(QSBWorldSync.GetWorldFromUnity<QSBQuantumState>).ToList();

				if (QuantumStates.Any(x => x == null))
				{
					DebugLog.ToConsole($"Error - {AttachedObject.name} has one or more null QSBQuantumStates assigned!", OWML.Common.MessageType.Error);
				}
			});
		}

		public void ChangeState(int newStateIndex)
		{
			if (CurrentState != -1)
			{
				QuantumStates[CurrentState].SetVisible(false);
			}

			QuantumStates[newStateIndex].SetVisible(true);
			AttachedObject._stateIndex = newStateIndex;
			if (QSBCore.ShowQuantumDebugBoxes)
			{
				DebugBoxText.text = $"Multistate\r\nid:{ObjectId}\r\nstate:{CurrentState}";
			}
		}
	}
}
