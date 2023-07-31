using QSB.EyeOfTheUniverse.EyeStateSync.Messages;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.EyeOfTheUniverse.MaskSync;

public class MaskManager : MonoBehaviour, IAddComponentOnStart
{
	private static bool _flickering;
	private static float _flickerOutTime;

	public static List<PlayerInfo> WentOnSolanumsWildRide = new();

	private void Awake() => QSBSceneManager.OnSceneLoaded += OnSceneLoaded;

	private static void OnSceneLoaded(OWScene oldScene, OWScene newScene, bool inUniverse)
	{
		_flickering = false;
		_flickerOutTime = 0f;
	}

	public static void FlickerOutShuttle()
	{
		FlickerMessage.IgnoreNextMessage = true;
		GlobalMessenger<float, float>.FireEvent(OWEvents.FlickerOffAndOn, 0.5f, 0.5f);
		_flickerOutTime = Time.time + 0.5f;
		_flickering = true;

		// hide all players in shuttle
		QSBPlayerManager.PlayerList.Where(x => x.IsInEyeShuttle).ForEach(x => x.SetVisible(false));
	}

	private void Update()
	{
		if (_flickering && Time.time > _flickerOutTime)
		{
			var controller = QSBWorldSync.GetUnityObject<EyeShuttleController>();
			controller._shuttleObject.SetActive(false);
			_flickering = false;
			_flickerOutTime = 0f;
		}
	}
}