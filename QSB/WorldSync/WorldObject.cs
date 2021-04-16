using QSB.TransformSync;
using UnityEngine;

namespace QSB.WorldSync
{
	public abstract class WorldObject<T> : IWorldObject
		where T : MonoBehaviour
	{
		public int ObjectId { get; protected set; }
		public T AttachedObject { get; protected set; }
		public string Name => AttachedObject == null ? "<NullObject!>" : AttachedObject.name;
		public WorldObjectTransformSync TransformSync { get; set; }

		public abstract void Init(T attachedObject, int id);
		public virtual void OnRemoval() { }
		public MonoBehaviour ReturnObject() => AttachedObject;
	}
}