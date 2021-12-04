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
	}
}
