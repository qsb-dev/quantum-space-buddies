using QSB.WorldSync;
using UnityEngine;

namespace QSB.ItemSync.WorldObjects
{
	class QSBOWItemSocket<T> : WorldObject<T>, IQSBOWItemSocket
		where T : MonoBehaviour
	{
		public override void Init(T attachedObject, int id) { }
	}
}
