using UnityEngine;

namespace QSB.Syncs
{
	public interface ISync<T>
	{
		Transform ReferenceTransform { get; }
		T AttachedObject { get; }

		bool IsReady { get; }
		bool UseInterpolation { get; }
	}
}
