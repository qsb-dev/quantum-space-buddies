using UnityEngine;

namespace QSB.WorldSync
{
	public interface IWorldObject
	{
		int ObjectId { get; }
		string Name { get; }
		MonoBehaviour AttachedObject { get; }

		void OnRemoval();
		bool ShouldDisplayDebug();
		string ReturnLabel();
		void DisplayLines();

		/// <summary>
		/// called on the host to send over initial state messages
		/// <para/>
		/// world objects will be ready on both sides at this point
		/// </summary>
		void SendInitialState(uint to);
	}
}