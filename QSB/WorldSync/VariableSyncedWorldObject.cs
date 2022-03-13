using QSB.Utility.VariableSync;
using UnityEngine;

namespace QSB.WorldSync;

internal abstract class VariableSyncedWorldObject<T, U> : WorldObject<T>
	where T : MonoBehaviour
	where U : IWorldObjectVariableSyncer
{
	protected U Syncer;

	public void SetSyncer(U syncer)
	{
		Syncer = syncer;
		Syncer.Init(this);
	}
}
