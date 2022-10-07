using QSB.Patches;
using QSB.Player;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.Spectate;

internal class SpectateManager : MonoBehaviour, IAddComponentOnStart
{
	public static SpectateManager Instance { get; private set; }

	private bool _isSpectating;
	private PlayerInfo _spectateTarget;

	public void Start()
	{
		Instance = this;
	}

	public void TriggerSpectate()
	{
		QSBPatchManager.DoPatchType(QSBPatchTypes.SpectateTime);

		var player = QSBPlayerManager.PlayerList.First(x => !x.IsLocalPlayer);
		SpectatePlayer(player);
		
		_isSpectating = true;
	}

	public void ExitSpectate()
	{
		QSBPatchManager.DoUnpatchType(QSBPatchTypes.SpectateTime);

		var prevDetectorGO = _spectateTarget.FluidDetector.gameObject;
		var prevSectorDetector = prevDetectorGO.GetComponent<SectorDetector>();
		Destroy(prevSectorDetector);
		_spectateTarget.Camera.enabled = false;
		_spectateTarget = null;

		var camera = QSBPlayerManager.LocalPlayer.Camera;
		GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", camera);
		camera.enabled = true;
		var cameraEffectController = camera.GetComponent<PlayerCameraEffectController>();
		cameraEffectController.OpenEyes(1f);
	}

	private void SpectatePlayer(PlayerInfo player)
	{
		DebugLog.DebugWrite($"Spectating {player}");
		GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", player.Camera);
		QSBPlayerManager.LocalPlayer.Camera.enabled = false;
		
		if (_spectateTarget != null)
		{
			_spectateTarget.Camera.enabled = false;
			var prevDetectorGO = _spectateTarget.FluidDetector.gameObject;
			var prevSectorDetector = prevDetectorGO.GetComponent<SectorDetector>();
			Destroy(prevSectorDetector);
		}

		_spectateTarget = player;
		player.Camera.enabled = true;

		var detectorGO = player.FluidDetector.gameObject;
		var sectorDetector = detectorGO.AddComponent<SectorDetector>();
		sectorDetector.SetOccupantType(DynamicOccupant.Player);

		foreach (var occupiedSector in player.TransformSync.SectorDetector.SectorList)
		{
			DebugLog.DebugWrite($"Adding {occupiedSector.AttachedObject.name} to sector detector");
			sectorDetector.AddSector(occupiedSector.AttachedObject);
		}
	}

	public void Update()
	{
		if (!_isSpectating)
		{
			return;
		}
	}
}
