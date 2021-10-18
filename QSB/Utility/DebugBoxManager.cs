using OWML.Common;
using UnityEngine;
using UnityEngine.UI;

namespace QSB.Utility
{
	public static class DebugBoxManager
	{
		private static GameObject _boxPrefab;

		public static void Init()
		{
			_boxPrefab = QSBCore.ConversationAssetBundle.LoadAsset<GameObject>("assets/dialoguebubble.prefab");
			var font = (Font)Resources.Load(@"fonts\english - latin\spacemono-bold");
			if (font == null)
			{
				DebugLog.ToConsole("Error - Font is null!", MessageType.Error);
			}

			_boxPrefab.GetComponent<Text>().font = font;
			_boxPrefab.GetComponent<Text>().color = Color.white;
		}

		public static GameObject CreateBox(Transform parent, float vertOffset, string text)
		{
			var newBox = Object.Instantiate(_boxPrefab);
			newBox.SetActive(false);
			newBox.transform.SetParent(parent);
			newBox.transform.localPosition = new Vector3(0, vertOffset, 0);
			newBox.transform.rotation = parent.rotation;
			newBox.AddComponent<CameraFacingBillboard>();
			newBox.GetComponent<Text>().text = text;
			newBox.AddComponent<ZOverride>();
			newBox.SetActive(true);
			return newBox;
		}
	}
}