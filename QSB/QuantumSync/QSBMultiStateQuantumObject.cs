using OWML.Utils;
using QSB.WorldSync;

namespace QSB.QuantumSync
{
	public class QSBMultiStateQuantumObject : WorldObject<MultiStateQuantumObject>
	{
		public QuantumState[] QuantumStates { get; private set; }

		public override void Init(MultiStateQuantumObject attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;
			QuantumStates = AttachedObject.GetValue<QuantumState[]>("_states");
		}
	}
}
