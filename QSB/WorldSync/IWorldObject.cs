using UnityEngine;

namespace QSB.WorldSync
{
	public interface IWorldObject
	{
		int ObjectId { get; }
		WorldObjectManager Manager { get; }
		string Name { get; }

		void OnRemoval();
		MonoBehaviour ReturnObject();
		bool ShouldDisplayLabel { get; }
		string ReturnLabel();
	}
}
