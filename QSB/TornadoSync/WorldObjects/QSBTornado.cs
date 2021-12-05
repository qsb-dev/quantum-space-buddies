using QSB.Utility;
using QSB.WorldSync;

namespace QSB.TornadoSync.WorldObjects
{
	public class QSBTornado : WorldObject<TornadoController>
	{
		public override void Init(TornadoController attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;
		}

		public void FormCollapse(bool formCollapse)
		{
			if (formCollapse)
			{
				AttachedObject.StartFormation();
				DebugLog.DebugWrite($"{LogName} form");
			}
			else
			{
				AttachedObject.StartCollapse();
				DebugLog.DebugWrite($"{LogName} collapse");
			}
		}
	}
}
