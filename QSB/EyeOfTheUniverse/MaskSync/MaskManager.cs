using QSB.EyeOfTheUniverse.EyeStateSync.Messages;
using QSB.Player;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.EyeOfTheUniverse.MaskSync
{
	internal class MaskManager : MonoBehaviour
	{
		private static bool _flickering;
		private static float _flickerOutTime;

		public void Awake() => QSBPlayerManager.OnRemovePlayer += OnPlayerLeave;

		private static void OnPlayerLeave(PlayerInfo player)
		{
			if (player == QSBPlayerManager.LocalPlayer)
			{
				_flickering = false;
				_flickerOutTime = 0f;
			}
		}

		public static void FlickerOutShuttle()
		{
			FlickerMessage.IgnoreNextMessage = true;
			GlobalMessenger<float, float>.FireEvent("FlickerOffAndOn", 0.5f, 0.5f);
			_flickerOutTime = Time.time + 0.5f;
			_flickering = true;
		}

		private void Update()
		{
			if (_flickering && Time.time > _flickerOutTime)
			{
				var controller = QSBWorldSync.GetUnityObjects<EyeShuttleController>().First();
				controller._shuttleObject.SetActive(false);
				_flickering = false;
				_flickerOutTime = 0f;
			}
		}
	}
}
