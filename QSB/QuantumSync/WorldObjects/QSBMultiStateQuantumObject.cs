using OWML.Utils;
using QSB.Utility;
using UnityEngine.UI;

namespace QSB.QuantumSync.WorldObjects
{
	internal class QSBMultiStateQuantumObject : QSBQuantumObject<MultiStateQuantumObject>
	{
		public QuantumState[] QuantumStates { get; private set; }
		public Text DebugBoxText;

		public override void Init(MultiStateQuantumObject attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;
			QuantumStates = AttachedObject.GetValue<QuantumState[]>("_states");
			if (QSBCore.DebugMode)
			{
				DebugBoxText = DebugBoxManager.CreateBox(AttachedObject.transform, 0, AttachedObject.GetValue<int>("_stateIndex").ToString()).GetComponent<Text>();
			}
			base.Init(attachedObject, id);
		}

		public void ChangeState(int stateIndex)
		{
			var currentStateIndex = AttachedObject.GetValue<int>("_stateIndex");
			if (currentStateIndex != -1)
			{
				QuantumStates[currentStateIndex].SetVisible(false);
			}
			QuantumStates[stateIndex].SetVisible(true);
			AttachedObject.SetValue("_stateIndex", stateIndex);
			if (QSBCore.DebugMode)
			{
				DebugBoxText.text = stateIndex.ToString();
			}
		}
	}
}
