using UnityEngine;

namespace QSB.Utility
{
	internal class DebugCameraSettings : MonoBehaviour, IAddComponentOnStart
	{
		public static void UpdateFromDebugSetting()
		{
			if (QSBCore.DebugSettings.GreySkybox)
			{
				QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
				Camera.main.backgroundColor = Color.gray;
			}
			else
			{
				QSBSceneManager.OnSceneLoaded -= OnSceneLoaded;
				Camera.main.backgroundColor = _origColor;
			}
		}

		private static Color _origColor;

		private void Start()
		{
			_origColor = Camera.main.backgroundColor;
			UpdateFromDebugSetting();
		}

		private static void OnSceneLoaded(OWScene arg1, OWScene arg2, bool arg3)
			=> Camera.main.backgroundColor = Color.gray;
	}
}
