using UnityEngine;

namespace QSB.Utility;

public class DebugCameraSettings : MonoBehaviour, IAddComponentOnStart
{
	public static void UpdateFromDebugSetting()
	{
		QSBSceneManager.OnSceneLoaded -= OnSceneLoaded;
		if (Camera.main)
		{
			Camera.main.backgroundColor = _origColor;
		}

		if (!QSBCore.DebugSettings.GreySkybox)
		{
			return;
		}

		QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
		if (Camera.main)
		{
			Camera.main.backgroundColor = Color.gray;
		}
	}

	private static Color _origColor;

	private void Awake()
	{
		_origColor = Camera.main.backgroundColor;
		UpdateFromDebugSetting();
		Destroy(this);
	}

	private static void OnSceneLoaded(OWScene arg1, OWScene arg2, bool arg3)
		=> Camera.main.backgroundColor = Color.gray;
}
