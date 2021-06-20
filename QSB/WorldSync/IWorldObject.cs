using UnityEngine;

namespace QSB.WorldSync
{
	public interface IWorldObject
	{
		int ObjectId { get; }
		string Name { get; }

		void OnRemoval();
		MonoBehaviour ReturnObject();
	}
}
