using QSB.TransformSync;
using UnityEngine;

namespace QSB.WorldSync
{
	public interface IWorldObject
	{
		int ObjectId { get; }
		string Name { get; }
		WorldObjectTransformSync TransformSync { get; set; }

		void OnRemoval();
		MonoBehaviour ReturnObject();
	}
}
