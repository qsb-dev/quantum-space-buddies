using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.AlarmTotemSync.Messages;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Threading;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;

public class QSBAlarmTotem : WorldObject<AlarmTotem>
{
	private readonly List<uint> _visibleFor = new();

	public override void SendInitialState(uint to)
	{
		this.SendMessage(new SetFaceOpenMessage(AttachedObject._isFaceOpen) { To = to });
		this.SendMessage(new SetEnabledMessage(AttachedObject.enabled) { To = to });
		foreach (var playerId in _visibleFor)
		{
			this.SendMessage(new SetVisibleMessage(playerId, true));
		}
	}

	public override async UniTask Init(CancellationToken ct)
	{
		QSBPlayerManager.OnRemovePlayer += OnPlayerLeave;

		Delay.RunWhen(() => QSBWorldSync.AllObjectsReady, () =>
		{
			if (AttachedObject._isPlayerVisible)
			{
				this.SendMessage(new SetVisibleMessage(true));
			}
		});
	}

	public override void OnRemoval() =>
		QSBPlayerManager.OnRemovePlayer -= OnPlayerLeave;

	private void OnPlayerLeave(PlayerInfo player) =>
		_visibleFor.QuickRemove(player.PlayerId);

	public bool IsVisible() => _visibleFor.Count > 0;

	public void SetVisible(uint playerId, bool visible)
	{
		if (visible)
		{
			_visibleFor.SafeAdd(playerId);
		}
		else
		{
			_visibleFor.QuickRemove(playerId);
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
			AttachedObject._pulseLightController.SetIntensity(0f);
			AttachedObject._simTotemMaterials[0] = AttachedObject._origSimEyeMaterial;
			AttachedObject._simTotemRenderer.sharedMaterials = AttachedObject._simTotemMaterials;
			AttachedObject._simVisionConeRenderer.SetColor(AttachedObject._simVisionConeRenderer.GetOriginalColor());
			if (AttachedObject._isPlayerVisible)
			{
				AttachedObject._isPlayerVisible = false;
				Locator.GetAlarmSequenceController().DecreaseAlarmCounter();
			}
		}
	}

	#region local visibility

	private bool _isLocallyVisible;

	public void FixedUpdate()
	{
		var isLocallyVisible = _isLocallyVisible;
		_isLocallyVisible = CheckPlayerVisible();
		if (_isLocallyVisible && !isLocallyVisible)
		{
			this.SendMessage(new SetVisibleMessage(true));
		}
		else if (isLocallyVisible && !_isLocallyVisible)
		{
			this.SendMessage(new SetVisibleMessage(false));
		}
	}

	private bool CheckPlayerVisible()
	{
		if (!AttachedObject._isFaceOpen)
		{
			return false;
		}

		var lanternController = Locator.GetDreamWorldController().GetPlayerLantern().GetLanternController();
		var playerLightSensor = Locator.GetPlayerLightSensor();
		if (lanternController.IsHeldByPlayer() && !lanternController.IsConcealed() || playerLightSensor.IsIlluminated())
		{
			var position = Locator.GetPlayerCamera().transform.position;
			if (AttachedObject.CheckPointInVisionCone(position) && !AttachedObject.CheckLineOccluded(AttachedObject._sightOrigin.position, position))
			{
				return true;
			}
		}

		return false;
	}

	#endregion
}
