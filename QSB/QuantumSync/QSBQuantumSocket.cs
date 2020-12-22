using OWML.Utils;
using QSB.WorldSync;

namespace QSB.QuantumSync
{
	internal class QSBQuantumSocket : WorldObject
	{
		public QuantumSocket AttachedSocket { get; private set; }

		public void Init(QuantumSocket quantumSocket, int id)
		{
			ObjectId = id;
			AttachedSocket = quantumSocket;

			AttachedSocket.GetType().SetValue("_randomYRotation", false);
		}
	}
}
