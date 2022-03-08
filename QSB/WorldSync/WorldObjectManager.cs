using Cysharp.Threading.Tasks;
using QSB.Utility;
using System.Threading;
using UnityEngine;

namespace QSB.WorldSync;

public enum WorldObjectScene
{
	Both,
	SolarSystem,
	Eye
}

public abstract class WorldObjectManager : MonoBehaviour, IAddComponentOnStart
{
	public abstract WorldObjectScene WorldObjectScene { get; }

	public abstract UniTask BuildWorldObjects(OWScene scene, CancellationToken ct);

	public virtual void UnbuildWorldObjects() { }

	public override string ToString() => GetType().ToString();
}