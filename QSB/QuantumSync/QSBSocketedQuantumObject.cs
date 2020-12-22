using QSB.WorldSync;

namespace QSB.QuantumSync
{
	internal class QSBSocketedQuantumObject : WorldObject
	{
		public SocketedQuantumObject AttachedObject { get; private set; }

		public void Init(SocketedQuantumObject quantumObject, int id)
		{
			ObjectId = id;
			AttachedObject = quantumObject;
		}
	}
}
