using UnityEngine;

namespace QSB.WorldSync
{
	public interface IWorldObject
	{
		int ObjectId { get; }
		string Name { get; }

		void OnRemoval();
		MonoBehaviour ReturnObject();
		bool ShouldDisplayDebug();
		string ReturnLabel();
		void DisplayLines();

		void SendResyncInfo(uint to);
	}
}
