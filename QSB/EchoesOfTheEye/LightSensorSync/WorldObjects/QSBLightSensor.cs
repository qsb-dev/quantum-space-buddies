using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.LightSensorSync.Messages;
using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;
using System;
using System.Threading;

namespace QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;

internal class QSBLightSensor : WorldObject<SingleLightSensor>
{
	public uint AuthorityOwner;

	public bool LocallyIlluminated;

	public Action OnDetectLocalLight;
	public Action OnDetectLocalDarkness;

	public override void SendInitialState(uint to) =>
		this.SendMessage(new LightSensorAuthorityMessage(AuthorityOwner) { To = to });

	public override async UniTask Init(CancellationToken ct) => QSBPlayerManager.OnRemovePlayer += OnPlayerLeave;
	public override void OnRemoval() => QSBPlayerManager.OnRemovePlayer -= OnPlayerLeave;

	private void OnPlayerLeave(PlayerInfo player)
	{
		if (AuthorityOwner == player.PlayerId)
		{
			// player left with authority, give it to us if we can
			this.SendMessage(new LightSensorAuthorityMessage(AttachedObject.enabled ? QSBPlayerManager.LocalPlayerId : 0));
		}
	}
}
