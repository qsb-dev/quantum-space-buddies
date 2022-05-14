using QSB.EyeOfTheUniverse.Tomb.Messages;
using QSB.Messaging;
using QSB.Player;
using UnityEngine;

namespace QSB.EyeOfTheUniverse.Tomb;

internal class EyeTombWatcher : MonoBehaviour
{
	private EyeTombController tomb;
	private bool _observedGrave;

	private void Start()
	{
		tomb = GetComponent<EyeTombController>();
		tomb._graveObserveTrigger.OnGainFocus += OnObserveGrave;
	}

	private void OnDestroy()
		=> tomb._graveObserveTrigger.OnGainFocus -= OnObserveGrave;

	private void OnObserveGrave()
	{
		_observedGrave = true;
		tomb._graveObserveTrigger.OnGainFocus -= OnObserveGrave;
	}
	
	private void FixedUpdate()
	{
		if (!_observedGrave)
		{
			return;
		}

		var canShowStage = true;
		foreach (var player in QSBPlayerManager.PlayerList)
		{
			var playerToStage = tomb._stageRoot.transform.position - player.Body.transform.position;
			var playerLookDirection = player.Body.transform.forward;
			var angle = Vector3.Angle(playerLookDirection, playerToStage);
			if (angle < 70)
			{
				canShowStage = false;
			}
		}

		if (canShowStage)
		{
			tomb._stageRoot.SetActive(true);
			new ShowStageMessage().Send();
			enabled = false;
		}
	}
}
