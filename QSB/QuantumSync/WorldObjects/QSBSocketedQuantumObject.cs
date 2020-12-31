using QSB.WorldSync;

namespace QSB.QuantumSync.WorldObjects
{
	internal class QSBSocketedQuantumObject : WorldObject<SocketedQuantumObject>
	{
		public override void Init(SocketedQuantumObject quantumObject, int id)
		{
			ObjectId = id;
			AttachedObject = quantumObject;
		}
	}
}