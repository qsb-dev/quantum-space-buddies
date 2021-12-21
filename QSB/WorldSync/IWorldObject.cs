using UnityEngine;

namespace QSB.WorldSync
{
	public interface IWorldObject
	{
		public WorldObjectType WorldObjectType { get; }
		int ObjectId { get; }
		string Name { get; }

		void OnRemoval();
		MonoBehaviour ReturnObject();
	}
}
