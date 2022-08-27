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

public class QSBStationaryProbeLauncher : QSBProbeLauncher, ILinkedWorldObject<StationaryProbeLauncherVariableSyncer>
{
	private uint _currentUser = uint.MaxValue;

	public StationaryProbeLauncherVariableSyncer NetworkBehaviour { get; private set; }
	public void SetNetworkBehaviour(NetworkBehaviour networkBehaviour) => NetworkBehaviour = (StationaryProbeLauncherVariableSyncer)networkBehaviour;

	private bool _isInUse;
	private StationaryProbeLauncher _stationaryProbeLauncher;

	public override async UniTask Init(CancellationToken ct)
	{
		// This is implemented by inheriting LinkedWorldObject normally,
		// However I want to inherit from QSBProbeLauncher or else we'd have to redo the sync for the VFX/SFX
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

		// Fix spatial blend of sound effects
		_stationaryProbeLauncher._effects._owAudioSource.spatialBlend = 1;
		_stationaryProbeLauncher._audioSource.spatialBlend = 1;

		UpdateUse();
	}

	public override void OnRemoval()
	{
		if (QSBCore.IsHost)
		{
			NetworkServer.Destroy(NetworkBehaviour.gameObject);
		}

		base.OnRemoval();

		_stationaryProbeLauncher._interactVolume.OnPressInteract -= OnPressInteract;
	}

	private void OnPressInteract() => OnLocalUseStateChanged(true);

	public override void SendInitialState(uint to)
	{
		base.SendInitialState(to);

		this.SendMessage(new StationaryProbeLauncherMessage(_isInUse, _currentUser) { To = to });
	}

	public void OnRemoteUseStateChanged(bool isInUse, uint user)
	{
		_isInUse = isInUse;

		_currentUser = isInUse ? user : uint.MaxValue;

		// Whoever is using it needs authority to be able to rotate it
		if (QSBCore.IsHost)
		{
			NetworkBehaviour.netIdentity.SetAuthority(_currentUser);
		}

		UpdateUse();
	}

	public void OnLocalUseStateChanged(bool isInUse)
	{
		_isInUse = isInUse;

		_currentUser = isInUse ? QSBPlayerManager.LocalPlayerId : uint.MaxValue;

		// Whoever is using it needs authority to be able to rotate it
		if (QSBCore.IsHost)
		{
			NetworkBehaviour.netIdentity.SetAuthority(_currentUser);
		}

		this.SendMessage(new StationaryProbeLauncherMessage(isInUse, _currentUser));
	}

	private void UpdateUse()
	{
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
