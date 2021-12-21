using QSB.Player;
using UnityEngine;

namespace QSB.WorldSync
{
	public abstract class WorldObject<T> : IWorldObject
		where T : MonoBehaviour
	{
		public int ObjectId { get; init; }
		public T AttachedObject { get; init; }
		public string Name => AttachedObject == null ? "<NullObject!>" : AttachedObject.name;
		public string LogName => $"{QSBPlayerManager.LocalPlayerId}.{ObjectId}:{GetType().Name}";

		public virtual void Init() { }
		public virtual void OnRemoval() { }
		public MonoBehaviour ReturnObject() => AttachedObject;

		/// indicates that this won't become ready immediately
		protected void StartDelayedReady() => WorldObjectManager._numObjectsReadying++;

		/// indicates that this is now ready
		protected void FinishDelayedReady() => WorldObjectManager._numObjectsReadying--;
	}
}
