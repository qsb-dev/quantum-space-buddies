using UnityEngine;

namespace QSB.WorldSync;

public interface IWorldObject
{
	int ObjectId { get; }
	string Name { get; }
	MonoBehaviour AttachedObject { get; }

	void OnRemoval();
	bool ShouldDisplayDebug();
	string ReturnLabel();
	void DisplayLines();
}