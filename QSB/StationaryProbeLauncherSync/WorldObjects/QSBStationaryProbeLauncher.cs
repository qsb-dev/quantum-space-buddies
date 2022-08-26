using Cysharp.Threading.Tasks;
using Mirror;
using QSB.AuthoritySync;
using QSB.Messaging;
using QSB.Player;
using QSB.StationaryProbeLauncherSync.Messages;
using QSB.StationaryProbeLauncherSync.TransformSync;
using QSB.Tools.ProbeLauncherTool.WorldObjects;
using QSB.Utility.LinkedWorldObject;
using System.Threading;

namespace QSB.StationaryProbeLauncherSync.WorldObjects;

public class QSBStationaryProbeLauncher : QSBProbeLauncher, ILinkedWorldObject<StationaryProbeLauncherTransformSync>
{
	public StationaryProbeLauncherTransformSync NetworkBehaviour { get; private set; }
	public void SetNetworkBehaviour(NetworkBehaviour networkBehaviour) => NetworkBehaviour = (StationaryProbeLauncherTransformSync)networkBehaviour;

	private bool _isInit;
	private bool _isInUse;
	private Shape _shape;
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

		_stationaryProbeLauncher = AttachedObject as StationaryProbeLauncher;
		_shape = ((InteractZone)_stationaryProbeLauncher._interactVolume)._trigger._shape;

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
		if (QSBCore.IsHost) NetworkBehaviour.netIdentity.SetAuthority(QSBPlayerManager.LocalPlayerId);

		_isInUse = true;
		this.SendMessage(new StationaryProbeLauncherMessage(_isInUse));
	}

	public override void SendInitialState(uint to)
	{
		base.SendInitialState(to);

		this.SendMessage(new StationaryProbeLauncherMessage(_isInUse) { To = to });
	}

	private void UpdateUse()
	{
		// Stuff can be null when its sending the initial state info
		if (!_isInit) return;

		// If somebody is using this we disable the interaction shape
		_shape.enabled = !_isInUse;
		
		if (_isInUse)
		{
			_stationaryProbeLauncher._audioSource.SetLocalVolume(0f);
			_stationaryProbeLauncher._audioSource.Start();
		}
		else
		{
			_stationaryProbeLauncher._audioSource.Stop();
		}
	}

	public void OnUseStateChanged(bool isInUse, uint from)
	{
		// Whoever is using it needs authority to be able to rotate it
		if (QSBCore.IsHost) NetworkBehaviour.netIdentity.SetAuthority(from);

		_isInUse = isInUse;

		UpdateUse();
	}
}
