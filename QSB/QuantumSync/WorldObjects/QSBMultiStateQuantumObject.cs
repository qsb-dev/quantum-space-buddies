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
		public int CurrentState => AttachedObject.GetValue<int>("_stateIndex");

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
			QuantumStates = AttachedObject.GetValue<QuantumState[]>("_states").ToList().Select(x => QSBWorldSync.GetWorldFromUnity<QSBQuantumState, QuantumState>(x)).ToList();
			if (QSBCore.DebugMode)
			{
				DebugBoxText = DebugBoxManager.CreateBox(AttachedObject.transform, 0, CurrentState.ToString()).GetComponent<Text>();
			}
			base.Init(attachedObject, id);
		}

		public void ChangeState(int newStateIndex)
		{
			if (CurrentState != -1)
			{
				QuantumStates[CurrentState].SetVisible(false);
			}
			QuantumStates[newStateIndex].SetVisible(true);
			AttachedObject.SetValue("_stateIndex", newStateIndex);
			if (QSBCore.DebugMode)
			{
				DebugBoxText.text = newStateIndex.ToString();
			}
		}
	}
}
