using UnityEngine;

namespace QSB.WorldSync
{
	public abstract class WorldObject<T> : IWorldObject
		where T : MonoBehaviour
	{
		public int ObjectId { get; protected set; }
		public T AttachedObject { get; protected set; }
		public string Name => AttachedObject.name;

		public abstract void Init(T attachedObject, int id);
		public virtual void OnRemoval() { }
	}
}