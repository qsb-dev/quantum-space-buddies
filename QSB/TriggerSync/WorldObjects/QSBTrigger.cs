using Cysharp.Threading.Tasks;
using QSB.Messaging;
using QSB.Player;
using QSB.TriggerSync.Messages;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace QSB.TriggerSync.WorldObjects;

public interface IQSBTrigger : IWorldObject
{
	List<PlayerInfo> Occupants { get; }

	void Enter(PlayerInfo player);

	void Exit(PlayerInfo player);
}

public abstract class QSBTrigger<TO> : WorldObject<OWTriggerVolume>, IQSBTrigger
{
	public TO TriggerOwner { get; init; }

	public List<PlayerInfo> Occupants { get; } = new();

	protected virtual string CompareTag => "PlayerDetector";

	public override async UniTask Init(CancellationToken ct)
	{
		AttachedObject.OnEntry += OnEnterEvent;
		AttachedObject.OnExit += OnExitEvent;

		QSBPlayerManager.OnRemovePlayer += OnPlayerLeave;

		Delay.RunWhen(() => QSBWorldSync.AllObjectsReady, () =>
		{
			if (AttachedObject._trackedObjects != null && AttachedObject._trackedObjects.Any(x => x.CompareTag(CompareTag)))
			{
				((IQSBTrigger)this).SendMessage(new TriggerMessage(true));
			}
		});
	}

	public override void OnRemoval()
	{
		AttachedObject.OnEntry -= OnEnterEvent;
		AttachedObject.OnExit -= OnExitEvent;

		QSBPlayerManager.OnRemovePlayer -= OnPlayerLeave;
	}

	private void OnPlayerLeave(PlayerInfo player) => Exit(player);

	protected void OnEnterEvent(GameObject hitObj)
	{
		if (hitObj.CompareTag(CompareTag))
		{
			((IQSBTrigger)this).SendMessage(new TriggerMessage(true));
		}
	}

	protected void OnExitEvent(GameObject hitObj)
	{
		if (hitObj.CompareTag(CompareTag))
		{
			((IQSBTrigger)this).SendMessage(new TriggerMessage(false));
		}
	}

	public void Enter(PlayerInfo player)
	{
		if (!Occupants.SafeAdd(player))
		{
			return;
		}

		OnEnter(player);
	}

	public void Exit(PlayerInfo player)
	{
		if (!Occupants.QuickRemove(player))
		{
			return;
		}

		OnExit(player);
	}

	/// <summary>
	/// called when a player enters this trigger
	/// </summary>
	protected virtual void OnEnter(PlayerInfo player) { }

	/// <summary>
	/// called when a player exits this trigger or leaves the game
	/// </summary>
	protected virtual void OnExit(PlayerInfo player) { }
}
