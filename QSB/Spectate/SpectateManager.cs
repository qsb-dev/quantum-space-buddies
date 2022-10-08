using QSB.Patches;
using QSB.Player;
using QSB.Utility;
using System.Linq;
using UnityEngine;

namespace QSB.Spectate;

internal class SpectateManager : MonoBehaviour, IAddComponentOnStart
{
	public static SpectateManager Instance { get; private set; }

	public PlayerInfo SpectateTarget { get; private set; }

	private bool _isSpectating;
	

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

		OWInput.ChangeInputMode(InputMode.Map);
	}

	public void ExitSpectate()
	{
		QSBPatchManager.DoUnpatchType(QSBPatchTypes.SpectateTime);

		var prevDetectorGO = SpectateTarget.FluidDetector.gameObject;
		var prevSectorDetector = prevDetectorGO.GetComponent<SectorDetector>();
		Destroy(prevSectorDetector);
		SpectateTarget.Camera.enabled = false;
		SpectateTarget = null;

		var camera = QSBPlayerManager.LocalPlayer.Camera;
		GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", camera);
		camera.enabled = true;
		var cameraEffectController = camera.GetComponent<PlayerCameraEffectController>();
		cameraEffectController.OpenEyes(1f);

		OWInput.RestorePreviousInputs();
	}

	private void SpectatePlayer(PlayerInfo player)
	{
		DebugLog.DebugWrite($"Spectating {player}");
		GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", player.Camera);
		QSBPlayerManager.LocalPlayer.Camera.enabled = false;
		
		if (SpectateTarget != null)
		{
			SpectateTarget.Camera.enabled = false;
			var prevDetectorGO = SpectateTarget.FluidDetector.gameObject;
			var prevSectorDetector = prevDetectorGO.GetComponent<SectorDetector>();
			Destroy(prevSectorDetector);
		}

		SpectateTarget = player;
		player.Camera.enabled = true;

		var detectorGO = player.FluidDetector.gameObject;
		var sectorDetector = detectorGO.AddComponent<SectorDetector>();
		sectorDetector.SetOccupantType(DynamicOccupant.Player);
	}
}
