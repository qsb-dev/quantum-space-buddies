using UnityEngine;

namespace QSB.WorldSync;

internal class MonoBehaviourWorldObject : WorldObject<MonoBehaviour>
{
	public override void SendInitialState(uint to) { }
}
