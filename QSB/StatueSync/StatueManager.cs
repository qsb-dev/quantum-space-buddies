using QSB.Player;
using QSB.Player.TransformSync;
using QSB.Utility;
using System.Collections;
using UnityEngine;

namespace QSB.StatueSync;

public class StatueManager : MonoBehaviour, IAddComponentOnStart
{
	public static StatueManager Instance { get; private set; }
	public bool HasStartedStatueLocally;

	private void Awake()
	{
		Instance = this;
		QSBSceneManager.OnUniverseSceneLoaded += OnUniverseSceneLoaded;
	}

	private void OnDestroy()
		=> QSBSceneManager.OnUniverseSceneLoaded -= OnUniverseSceneLoaded;

	private void OnUniverseSceneLoaded(OWScene oldScene, OWScene newScene)
	{
		if (!QSBCore.IsInMultiplayer)
		{
			return;
		}

		if (PlayerTransformSync.LocalInstance == null)
		{
			DebugLog.ToConsole($"Error - Tried to run OnUniverseSceneLoaded when PlayerTransformSync.LocalInstance was null!", OWML.Common.MessageType.Error);
			return;
		}

		QSBPlayerManager.ShowAllPlayers();
		QSBPlayerManager.LocalPlayer.UpdateStatesFromObjects();
	}

	public void BeginSequence(Vector3 position, Quaternion rotation, float cameraDegrees) => StartCoroutine(BeginRemoteUplinkSequence(position, rotation, cameraDegrees));

	private IEnumerator BeginRemoteUplinkSequence(Vector3 position, Quaternion rotation, float cameraDegrees)
	{
		HasStartedStatueLocally = true;
		var cameraEffectController = Locator.GetPlayerCamera().GetComponent<PlayerCameraEffectController>();
		cameraEffectController.CloseEyes(0.5f);
		OWInput.ChangeInputMode(InputMode.None);
		Locator.GetPauseCommandListener().AddPauseCommandLock();
		Locator.GetToolModeSwapper().UnequipTool();
		Locator.GetFlashlight().TurnOff(false);
		yield return new WaitForSeconds(0.5f);
		// go to position
		QSBPlayerManager.HideAllPlayers();
		var timberHearth = Locator.GetAstroObject(AstroObject.Name.TimberHearth).GetAttachedOWRigidbody();
		Locator.GetPlayerBody().transform.position = timberHearth.transform.TransformPoint(position);
		Locator.GetPlayerBody().transform.rotation = timberHearth.transform.rotation * rotation;
		Locator.GetPlayerCamera().GetComponent<PlayerCameraController>().SetDegreesY(cameraDegrees);
		cameraEffectController.OpenEyes(1f, true);
		var uplinkTrigger = FindObjectOfType<MemoryUplinkTrigger>();
		uplinkTrigger.StartCoroutine("BeginUplinkSequence");
		yield break;
	}
}