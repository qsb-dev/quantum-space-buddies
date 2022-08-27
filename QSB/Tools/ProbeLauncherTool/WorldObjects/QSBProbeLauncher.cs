using Cysharp.Threading.Tasks;
using QSB.Messaging;
using QSB.Player;
using QSB.QuantumSync;
using QSB.Tools.ProbeLauncherTool.Messages;
using QSB.Tools.ProbeTool;
using QSB.WorldSync;
using System.Threading;
using UnityEngine;

namespace QSB.Tools.ProbeLauncherTool.WorldObjects;

public class QSBProbeLauncher : WorldObject<ProbeLauncher>
{
	private uint _probeOwnerID = uint.MaxValue;
	protected QSBProbe LaunchedProbe { get; private set; }

	public override async UniTask Init(CancellationToken ct) =>
		AttachedObject.OnLaunchProbe += OnLaunchProbe;

	public override void OnRemoval() =>
		AttachedObject.OnLaunchProbe -= OnLaunchProbe;

	public override void SendInitialState(uint to)
	{
		// Retrieval resets the probe owner ID
		var probeOwnerID = _probeOwnerID;

		if (AttachedObject._preLaunchProbeProxy.activeSelf)
		{
			this.SendMessage(new RetrieveProbeMessage(false));
		}
		else
		{
			this.SendMessage(new LaunchProbeMessage(false, probeOwnerID));
		}
	}

	private void OnLaunchProbe(SurveyorProbe probe) =>
		this.SendMessage(new LaunchProbeMessage(true, QSBPlayerManager.LocalPlayerId));

	public void RetrieveProbe(bool playEffects)
	{
		_probeOwnerID = uint.MaxValue;
		LaunchedProbe = null;

		if (AttachedObject._preLaunchProbeProxy.activeSelf)
		{
			return;
		}

		AttachedObject._preLaunchProbeProxy.SetActive(true);
		if (playEffects)
		{
			AttachedObject._effects.PlayRetrievalClip();
			AttachedObject._probeRetrievalEffect.WarpObjectIn(AttachedObject._probeRetrievalLength);
		}
	}

	public void LaunchProbe(bool playEffects, uint probeOwnerID)
	{
		_probeOwnerID = probeOwnerID;
		LaunchedProbe = QSBPlayerManager.GetPlayer(_probeOwnerID)?.Probe;

		if (LaunchedProbe == null) Debug.LogError($"Could not find probe owner with ID {_probeOwnerID}");

		if (!AttachedObject._preLaunchProbeProxy.activeSelf)
		{
			return;
		}

		AttachedObject._preLaunchProbeProxy.SetActive(false);

		if (playEffects)
		{
			// TODO : make this do underwater stuff correctly
			AttachedObject._effects.PlayLaunchClip(false);
			AttachedObject._effects.PlayLaunchParticles(false);
		}
	}

	public void ChangeMode()
	{
		AttachedObject._effects.PlayChangeModeClip();
	}

	public void TakeSnapshot(PlayerInfo player, ProbeCamera.ID cameraId)
	{
		// Not using PlaySnapshotClip because that uses Locator.GetPlayerAudioController() instead of owAudioSource for some reason
		AttachedObject._effects._owAudioSource.PlayOneShot(AudioType.ToolProbeTakePhoto, 1f);

		// If their probe is launched also play a snapshot from it
		if (LaunchedProbe && LaunchedProbe.IsLaunched()) LaunchedProbe.TakeSnapshot();

		QuantumManager.OnTakeProbeSnapshot(player, cameraId);
	}
}