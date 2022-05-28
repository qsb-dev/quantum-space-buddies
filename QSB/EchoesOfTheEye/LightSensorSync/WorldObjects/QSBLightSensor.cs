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

	internal bool _clientIlluminated;
	internal List<uint> _illuminatedBy;

	public override void SendInitialState(uint to) { }

	public override async UniTask Init(CancellationToken ct) => QSBPlayerManager.OnRemovePlayer += OnPlayerLeave;
	public override void OnRemoval() => QSBPlayerManager.OnRemovePlayer -= OnPlayerLeave;

	private void OnPlayerLeave(PlayerInfo player) =>
		_illuminatedBy.QuickRemove(player.PlayerId);
}
