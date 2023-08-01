using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace QSB.WorldSync;

public abstract class WorldObject<T> : IWorldObject
	where T : MonoBehaviour
{
	public int ObjectId { get; init; }
	MonoBehaviour IWorldObject.AttachedObject => AttachedObject;
	public T AttachedObject { get; init; }
	public string Name => AttachedObject ? AttachedObject.name : "<NullObject!>";
	public override string ToString() => $"{ObjectId}:{GetType().Name} ({Name})";

	public virtual async UniTask Init(CancellationToken ct) { }
	public virtual void OnRemoval() { }
	public virtual bool ShouldDisplayDebug() => QSBWorldSync.AllObjectsReady && AttachedObject && AttachedObject.gameObject.activeInHierarchy;
	public virtual string ReturnLabel() => ToString();
	public virtual void DisplayLines() { }
}