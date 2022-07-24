using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.LightSensorSync.Messages;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Threading;

/*
 * For those who come here,
 * leave while you still can.
 */

namespace QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;

internal class QSBLightSensor : WorldObject<SingleLightSensor>
{
	internal bool _locallyIlluminated;

	public Action OnDetectLocalLight;
	public Action OnDetectLocalDarkness;

	internal readonly List<uint> _illuminatedBy = new();

	public override void SendInitialState(uint to)
	{
		this.SendMessage(new IlluminatedByMessage(_illuminatedBy.ToArray()) { To = to });
		if (AttachedObject._illuminatingDreamLanternList != null)
		{
			this.SendMessage(new IlluminatingLanternsMessage(AttachedObject._illuminatingDreamLanternList) { To = to });
		}
	}

	public override async UniTask Init(CancellationToken ct)
	{
		QSBPlayerManager.OnRemovePlayer += OnPlayerLeave;

		// normally done in Start, but world objects arent ready by that point
		Delay.RunWhen(() => QSBWorldSync.AllObjectsReady, () =>
		{
			if (AttachedObject._sector != null)
			{
				if (AttachedObject._startIlluminated)
				{
					_locallyIlluminated = true;
					OnDetectLocalLight?.Invoke();
					this.SendMessage(new SetIlluminatedMessage(true));
				}
			}
		});
	}

	public override void OnRemoval() => QSBPlayerManager.OnRemovePlayer -= OnPlayerLeave;
	private void OnPlayerLeave(PlayerInfo player) => SetIlluminated(player.PlayerId, false);

	public void SetIlluminated(uint playerId, bool locallyIlluminated)
	{
		var illuminated = _illuminatedBy.Count > 0;
		if (locallyIlluminated)
		{
			_illuminatedBy.SafeAdd(playerId);
		}
		else
		{
			_illuminatedBy.QuickRemove(playerId);
		}

		if (!illuminated && _illuminatedBy.Count > 0)
		{
			AttachedObject._illuminated = true;
			AttachedObject.OnDetectLight.Invoke();
		}
		else if (illuminated && _illuminatedBy.Count == 0)
		{
			AttachedObject._illuminated = false;
			AttachedObject.OnDetectDarkness.Invoke();
		}
	}
}
