using OWML.Utils;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
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

			if (QSBCore.DebugMode)
			{
				DebugBoxText = DebugBoxManager.CreateBox(AttachedObject.transform, 0, $"Multistate\r\nid:{id}\r\nstate:{CurrentState}").GetComponent<Text>();
			}

			base.Init(attachedObject, id);
		}

		public override void PostInit()
		{
			QuantumStates = AttachedObject._states.ToList().Select(x => QSBWorldSync.GetWorldFromUnity<QSBQuantumState, QuantumState>(x)).ToList();

			if (QuantumStates.Any(x => x == null))
			{
				DebugLog.ToConsole($"Error - {AttachedObject.name} has one or more null QSBQuantumStates assigned!", OWML.Common.MessageType.Error);
			}
		}

		public void ChangeState(int newStateIndex)
		{
			if (CurrentState != -1)
			{
				QuantumStates[CurrentState].SetVisible(false);
			}

			QuantumStates[newStateIndex].SetVisible(true);
			AttachedObject._stateIndex = newStateIndex;
			if (QSBCore.DebugMode)
			{
				DebugBoxText.text = $"Multistate\r\nid:{ObjectId}\r\nstate:{CurrentState}";
			}
		}
	}
}
