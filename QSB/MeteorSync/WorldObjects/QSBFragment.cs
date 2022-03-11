using Cysharp.Threading.Tasks;
using QSB.Messaging;
using QSB.MeteorSync.Messages;
using QSB.WorldSync;
using System.Threading;
using UnityEngine;

namespace QSB.MeteorSync.WorldObjects;

public class QSBFragment : WorldObject<FragmentIntegrity>
{
	public override async UniTask Init(CancellationToken ct)
	{
		DetachableFragment = AttachedObject.GetComponent<DetachableFragment>();

		if (QSBCore.IsHost)
		{
			LeashLength = Random.Range(MeteorManager.WhiteHoleVolume._debrisDistMin, MeteorManager.WhiteHoleVolume._debrisDistMax);
		}
	}

	public override void SendInitialState(uint to) =>
		this.SendMessage(new FragmentInitialStateMessage(this) { To = to });

	public DetachableFragment DetachableFragment;
	public bool IsDetached => DetachableFragment != null && DetachableFragment._isDetached;
	public bool IsThruWhiteHole => IsDetached && DetachableFragment._sector != null &&
	                               DetachableFragment._sector._parentSector == MeteorManager.WhiteHoleVolume._whiteHoleSector;
	public OWRigidbody RefBody => IsThruWhiteHole ? MeteorManager.WhiteHoleVolume._whiteHoleBody : Locator._brittleHollow._owRigidbody;
	public OWRigidbody Body => IsDetached ? AttachedObject.transform.parent.parent.GetAttachedOWRigidbody() : null;

	/// <summary>
	/// what the leash length will be when we eventually detach and fall thru white hole
	/// </summary>
	public float? LeashLength;

	public void AddDamage(float damage)
		=> AttachedObject.AddDamage(damage);
}