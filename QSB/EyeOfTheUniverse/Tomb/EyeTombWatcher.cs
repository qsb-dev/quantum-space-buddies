using QSB.EyeOfTheUniverse.Tomb.Messages;
using QSB.Messaging;
using QSB.Player;
using UnityEngine;

namespace QSB.EyeOfTheUniverse.Tomb;

public class EyeTombWatcher : MonoBehaviour
{
	private EyeTombController _tomb;

	private void Awake()
	{
		_tomb = GetComponent<EyeTombController>();
		_tomb._graveObserveTrigger.OnGainFocus += OnObserveGrave;
		enabled = false;
	}

	private void OnDestroy() =>
		_tomb._graveObserveTrigger.OnGainFocus -= OnObserveGrave;

	private void OnObserveGrave()
	{
		_tomb._graveObserveTrigger.OnGainFocus -= OnObserveGrave;
		enabled = true;
	}

	private void FixedUpdate()
	{
		var canShowStage = true;
		foreach (var player in QSBPlayerManager.PlayerList)
		{
			var playerToStage = _tomb._stageRoot.transform.position - player.Body.transform.position;
			var playerLookDirection = player.Body.transform.forward;
			var angle = Vector3.Angle(playerLookDirection, playerToStage);
			if (angle < 70)
			{
				canShowStage = false;
			}
		}

		if (canShowStage)
		{
			_tomb._stageRoot.SetActive(true);
			new ShowStageMessage().Send();
			Destroy(this);
		}
	}
}
