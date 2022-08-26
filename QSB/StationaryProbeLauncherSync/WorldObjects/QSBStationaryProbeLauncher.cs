using Cysharp.Threading.Tasks;
using Mirror;
using QSB.AuthoritySync;
using QSB.Messaging;
using QSB.Player;
using QSB.StationaryProbeLauncherSync.Messages;
using QSB.StationaryProbeLauncherSync.VariableSync;
using QSB.Tools.ProbeLauncherTool.WorldObjects;
using QSB.Utility.LinkedWorldObject;
using System.Threading;

namespace QSB.StationaryProbeLauncherSync.WorldObjects;

public class QSBStationaryProbeLauncher : QSBProbeLauncher, ILinkedWorldObject<StationaryProbeLauncherVariableSync>
{
	public StationaryProbeLauncherVariableSync NetworkBehaviour { get; private set; }
	public void SetNetworkBehaviour(NetworkBehaviour networkBehaviour) => NetworkBehaviour = (StationaryProbeLauncherVariableSync)networkBehaviour;

	private bool _isInit;
	private bool _isInUse;
	private StationaryProbeLauncher _stationaryProbeLauncher;

	public override async UniTask Init(CancellationToken ct)
	{
		_isInit = true;

		// This is implemented by inheriting LinkedWorldObject normally, however I want to inherit from QSBProbeLauncher
		// Else we'd have to redo the sync for the effects
		if (QSBCore.IsHost)
		{
			this.SpawnLinked(QSBNetworkManager.singleton.StationaryProbeLauncherPrefab, false);
		}
		else
		{
			await this.WaitForLink(ct);
		}

		await base.Init(ct);

		_stationaryProbeLauncher = (StationaryProbeLauncher)AttachedObject;
		_stationaryProbeLauncher._interactVolume.OnPressInteract += OnPressInteract;

		UpdateUse();
	}

	public override void OnRemoval()
	{
		_isInit = false;

		if (QSBCore.IsHost)
		{
			NetworkServer.Destroy(NetworkBehaviour.gameObject);
		}

		base.OnRemoval();

		_stationaryProbeLauncher._interactVolume.OnPressInteract -= OnPressInteract;
	}

	private void OnPressInteract()
	{
		// Whoever is using it needs authority to be able to rotate it
		// If this is a client they'll get authority from the host when the message is received otherwise give now
		if (QSBCore.IsHost)
		{
			NetworkBehaviour.netIdentity.SetAuthority(QSBPlayerManager.LocalPlayerId);
		}

		_isInUse = true;
		this.SendMessage(new StationaryProbeLauncherMessage(_isInUse));
	}

	public override void SendInitialState(uint to)
	{
		base.SendInitialState(to);

		// BUG: will make host be the user and have authority instead of the actual user. can probably copy paste from UseSlideProjectorMessage
		this.SendMessage(new StationaryProbeLauncherMessage(_isInUse) { To = to });
	}

	public void OnRemoteUseStateChanged(bool isInUse, uint user)
	{
		// Whoever is using it needs authority to be able to rotate it
		if (QSBCore.IsHost)
		{
			NetworkBehaviour.netIdentity.SetAuthority(isInUse ? user : uint.MaxValue);
		}

		_isInUse = isInUse;

		UpdateUse();
	}

	private void UpdateUse()
	{
		// Stuff can be null when its sending the initial state info
		// BUG: uhhh how? this shouldnt be possible since initial state happens AFTER AllObjectsReady
		if (!_isInit)
		{
			return;
		}

		// If somebody is using this we disable the interaction shape
		_stationaryProbeLauncher._interactVolume.SetInteractionEnabled(!_isInUse);

		if (_isInUse)
		{
			_stationaryProbeLauncher._audioSource.SetLocalVolume(0f);
			_stationaryProbeLauncher._audioSource.Play();
		}
		else
		{
			_stationaryProbeLauncher._audioSource.Stop();
		}
	}
}
