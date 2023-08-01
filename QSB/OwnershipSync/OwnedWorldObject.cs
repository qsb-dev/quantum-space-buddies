using Cysharp.Threading.Tasks;
using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;
using System.Threading;
using UnityEngine;

namespace QSB.OwnershipSync;

/// <summary>
/// helper implementation of the interface
/// </summary>
public abstract class OwnedWorldObject<T> : WorldObject<T>, IOwnedWorldObject
	where T : MonoBehaviour
{
	public uint Owner { get; set; }
	public abstract bool CanOwn { get; }

	public override async UniTask Init(CancellationToken ct) =>
		QSBPlayerManager.OnRemovePlayer += OnPlayerLeave;

	public override void OnRemoval() =>
		QSBPlayerManager.OnRemovePlayer -= OnPlayerLeave;

	private void OnPlayerLeave(PlayerInfo player)
	{
		if (!QSBCore.IsHost)
		{
			return;
		}
		if (Owner == player.PlayerId)
		{
			((IOwnedWorldObject)this).SendMessage(new OwnedWorldObjectMessage(CanOwn ? QSBPlayerManager.LocalPlayerId : 0));
		}
	}
}
