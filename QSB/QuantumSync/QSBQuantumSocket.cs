using OWML.Utils;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using UnityEngine.UI;

namespace QSB.QuantumSync
{
	internal class QSBQuantumSocket : WorldObject<QuantumSocket>
	{
		public override void Init(QuantumSocket quantumSocket, int id)
		{
			ObjectId = id;
			AttachedObject = quantumSocket;
			AttachedObject.GetType().SetValue("_randomYRotation", false);
		}
	}
}