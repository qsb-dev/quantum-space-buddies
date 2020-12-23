using QSB.WorldSync;

namespace QSB.QuantumSync
{
	public class QSBMultiStateQuantumObject : WorldObject<MultiStateQuantumObject>
	{
		public override void Init(MultiStateQuantumObject attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;
		}
	}
}
