using QSB.TranslationSync.WorldObjects;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.TranslationSync
{
	internal class SpiralManager : MonoBehaviour
	{
		public static SpiralManager Instance { get; private set; }

		public void Awake()
		{
			Instance = this;
			QSBSceneManager.OnUniverseSceneLoaded += OnSceneLoaded;
		}

		public void OnDestroy() => QSBSceneManager.OnUniverseSceneLoaded -= OnSceneLoaded;

		private void OnSceneLoaded(OWScene scene)
		{
			QSBWorldSync.Init<QSBWallText, NomaiWallText>();
			QSBWorldSync.Init<QSBComputer, NomaiComputer>();
			QSBWorldSync.Init<QSBVesselComputer, NomaiVesselComputer>();
		}
	}
}
