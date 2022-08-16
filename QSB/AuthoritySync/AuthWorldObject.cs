using Cysharp.Threading.Tasks;
using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;
using System.Threading;
using UnityEngine;

namespace QSB.AuthoritySync;

/// <summary>
/// helper implementation of the interface
/// </summary>
public abstract class AuthWorldObject<T> : WorldObject<T>, IAuthWorldObject
	where T : MonoBehaviour
{
	public uint Owner { get; set; }
	public abstract bool CanOwn { get; }

	public override void SendInitialState(uint to) =>
		((IAuthWorldObject)this).SendMessage(new WorldObjectAuthMessage(Owner) { To = to });

	public override async UniTask Init(CancellationToken ct) =>
		QSBPlayerManager.OnRemovePlayer += OnPlayerLeave;

	public override void OnRemoval() =>
		QSBPlayerManager.OnRemovePlayer -= OnPlayerLeave;

	private void OnPlayerLeave(PlayerInfo player)
	{
		if (Owner == player.PlayerId)
		{
			// BUG: called once per player
			this.ReleaseOwnership();
		}
	}
}
