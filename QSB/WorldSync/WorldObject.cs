namespace QSB.WorldSync
{
	public abstract class WorldObject<T> where T : UnityEngine.Object
	{
		public int ObjectId { get; protected set; }
		public T AttachedObject { get; protected set; }

		public abstract void Init(T attachedObject, int id);
	}
}