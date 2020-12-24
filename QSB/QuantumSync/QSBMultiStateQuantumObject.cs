using OWML.Utils;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using UnityEngine.UI;

namespace QSB.QuantumSync
{
	public class QSBMultiStateQuantumObject : WorldObject<MultiStateQuantumObject>
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
		}
	}
}
