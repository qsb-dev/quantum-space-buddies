using OWML.Utils;
using QSB.WorldSync;

namespace QSB.QuantumSync.WorldObjects
{
	internal class QSBQuantumSocket : WorldObject<QuantumSocket>
	{
		public override void Init()
		{
			AttachedObject.GetType().SetValue("_randomYRotation", false);
		}
	}
}