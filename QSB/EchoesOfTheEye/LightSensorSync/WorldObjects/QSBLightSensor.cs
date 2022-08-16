using Cysharp.Threading.Tasks;
using QSB.AuthoritySync;
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

/// <summary>
/// TODO: switch this over to some sort of auth system.
/// list of illuminators doesn't work because if a player illuminates and then leaves,
/// it'll be considered illuminated forever until they come back.
///
/// BUG: this breaks in zone2.
/// the sector it's enabled in is bigger than the sector the zone2 walls are enabled in :(
/// maybe this can be fixed by making the collision group use the same sector.
/// </summary>
internal class QSBLightSensor : WorldObject<SingleLightSensor>, IAuthWorldObject
{
	internal bool _locallyIlluminated;
	public Action OnDetectLocalLight;
	public Action OnDetectLocalDarkness;


	public uint Owner { get; set; }
	public bool CanOwn => AttachedObject.enabled;


	public override void SendInitialState(uint to)
	{
		// todo initial state
	}

	public override async UniTask Init(CancellationToken ct)
	{
		QSBPlayerManager.OnRemovePlayer += OnPlayerLeave;

		// do this stuff here instead of Start, since world objects won't be ready by that point
		Delay.RunWhen(() => QSBWorldSync.AllObjectsReady, () =>
		{
			if (AttachedObject._sector != null)
			{
				if (AttachedObject._startIlluminated)
				{
					_locallyIlluminated = true;
					OnDetectLocalLight?.Invoke();
				}
			}
		});
	}

	public override void OnRemoval() => QSBPlayerManager.OnRemovePlayer -= OnPlayerLeave;
	private void OnPlayerLeave(PlayerInfo player) => SetIlluminated(player.PlayerId, false);

	public void SetIlluminated(uint playerId, bool locallyIlluminated)
	{
		// todo remove
	}
}
