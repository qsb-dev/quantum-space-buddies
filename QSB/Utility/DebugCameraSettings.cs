using UnityEngine;

namespace QSB.Utility
{
	internal class DebugCameraSettings : MonoBehaviour
	{
		void Start()
		{
			if (QSBCore.GreySkybox)
			{
				QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
				Camera.main.backgroundColor = Color.gray;
			}
		}

		private void OnSceneLoaded(OWScene arg1, OWScene arg2, bool arg3)
			=> Camera.main.backgroundColor = Color.gray;
	}
}
