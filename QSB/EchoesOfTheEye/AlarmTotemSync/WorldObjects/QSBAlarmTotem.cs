using Cysharp.Threading.Tasks;
using QSB.AuthoritySync;
using QSB.EchoesOfTheEye.AlarmTotemSync.Messages;
using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Threading;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;

/// <summary>
/// TODO: make this not NRE (by not doing enable sync) and then readd it back in
/// </summary>
public class QSBAlarmTotem : AuthWorldObject<AlarmTotem>
{
	public override bool CanOwn => AttachedObject.enabled;


	public readonly List<uint> VisibleFor = new();
	public bool IsLocallyVisible;

	public override void SendInitialState(uint to)
	{
		this.SendMessage(new TotemEnabledMessage(AttachedObject.enabled) { To = to });
		this.SendMessage(new TotemVisibleForMessage(VisibleFor) { To = to });
	}

	public override async UniTask Init(CancellationToken ct) =>
		QSBPlayerManager.OnRemovePlayer += OnPlayerLeave;

	public override void OnRemoval() =>
		QSBPlayerManager.OnRemovePlayer -= OnPlayerLeave;

	private void OnPlayerLeave(PlayerInfo player) =>
		VisibleFor.QuickRemove(player.PlayerId);

	public void SetVisible(uint playerId, bool visible)
	{
		if (visible)
		{
			VisibleFor.SafeAdd(playerId);
		}
		else
		{
			VisibleFor.QuickRemove(playerId);
		}
	}

	public void SetEnabled(bool enabled)
	{
		if (AttachedObject.enabled == enabled)
		{
			return;
		}

		if (!enabled &&
			AttachedObject._sector &&
			AttachedObject._sector.ContainsOccupant(DynamicOccupant.Player))
		{
			// local player is in sector, do not disable
			return;
		}

		AttachedObject.enabled = enabled;

		if (!enabled)
		{
			AttachedObject._simTotemMaterials[0] = AttachedObject._origSimEyeMaterial;
			AttachedObject._simTotemRenderer.sharedMaterials = AttachedObject._simTotemMaterials;
			AttachedObject._simVisionConeRenderer.SetColor(AttachedObject._simVisionConeRenderer.GetOriginalColor());
			AttachedObject._pulseLightController.SetIntensity(0f);
			/*
			if (AttachedObject._isPlayerVisible)
			{
				AttachedObject._isPlayerVisible = false;
				Locator.GetAlarmSequenceController().DecreaseAlarmCounter();
			}
			*/
			if (IsLocallyVisible)
			{
				IsLocallyVisible = false;
				this.SendMessage(new TotemVisibleMessage(false));
			}
		}
	}
}
