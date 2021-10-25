using UnityEngine;

namespace QSB.WorldSync
{
	public interface IWorldObject
	{
		int ObjectId { get; }
		string Name { get; }

		void PostInit();
		void OnRemoval();
		MonoBehaviour ReturnObject();
	}
}
