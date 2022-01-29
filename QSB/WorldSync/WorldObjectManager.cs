using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace QSB.WorldSync
{
	public enum WorldObjectType
	{
		Both,
		SolarSystem,
		Eye
	}

	public abstract class WorldObjectManager : MonoBehaviour
	{
		/// <summary>
		/// when the scene does not match the type, this manager will not build its world objects
		/// </summary>
		public abstract WorldObjectType WorldObjectType { get; }

		public abstract UniTask BuildWorldObjects(OWScene scene, CancellationToken ct);

		public virtual void UnbuildWorldObjects() { }

		public override string ToString() => GetType().Name;
	}
}
