using Cysharp.Threading.Tasks;
using QSB.Player;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Threading;

namespace QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;

internal class QSBLightSensor : WorldObject<SingleLightSensor>
{
	public bool LocallyIlluminated;
	public Action OnDetectLocalLight;
	public Action OnDetectLocalDarkness;

	internal bool _illuminated;
	private readonly List<uint> _illuminatedBy = new();

	public override void SendInitialState(uint to)
	{
		// todo
	}

	public override async UniTask Init(CancellationToken ct) => QSBPlayerManager.OnRemovePlayer += OnPlayerLeave;
	public override void OnRemoval() => QSBPlayerManager.OnRemovePlayer -= OnPlayerLeave;
	private void OnPlayerLeave(PlayerInfo player) => SetIlluminated(player.PlayerId, false);

	public void SetIlluminated(uint playerId, bool illuminated)
	{
		if (illuminated && !_illuminatedBy.SafeAdd(playerId))
		{
			// failed to add
			return;
		}

		if (!illuminated && !_illuminatedBy.QuickRemove(playerId))
		{
			// failed to remove
			return;
		}

		if (_illuminatedBy.Count > 0)
		{
			AttachedObject._illuminated = true;
			AttachedObject.OnDetectLight.Invoke();
		}
		else
		{
			AttachedObject._illuminated = false;
			AttachedObject.OnDetectDarkness.Invoke();
		}
	}
}
