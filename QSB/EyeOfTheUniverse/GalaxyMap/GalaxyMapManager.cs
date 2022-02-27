using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.EyeOfTheUniverse.GalaxyMap
{
	internal class GalaxyMapManager : MonoBehaviour, IAddComponentOnStart
	{
		public static GalaxyMapManager Instance { get; private set; }

		public QSBCharacterDialogueTree Tree { get; private set; }

		private void Awake()
		{
			Instance = this;
			QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
		}

		private void OnSceneLoaded(OWScene oldScene, OWScene newScene, bool inUniverse)
		{
			if (newScene != OWScene.EyeOfTheUniverse)
			{
				return;
			}

			var mapController = QSBWorldSync.GetUnityObjects<GalaxyMapController>().First();
			var map = mapController._interactVolume.gameObject;

			map.SetActive(false);
			Tree = map.AddComponent<QSBCharacterDialogueTree>();
			Tree._xmlCharacterDialogueAsset = QSBCore.TextAssetsBundle.LoadAsset<TextAsset>("Assets/TextAssets/GalaxyMap.txt");
			Tree._attentionPoint = map.transform;
			Tree._attentionPointOffset = new Vector3(0, 1, 0);
			Tree._turnOffFlashlight = true;
			Tree._turnOnFlashlight = true;
			map.SetActive(true);
		}
	}
}