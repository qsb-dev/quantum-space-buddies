using QSB.TranslationSync.WorldObjects;
using QSB.WorldSync;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.TranslationSync
{
	internal class SpiralManager : MonoBehaviour
	{
		public static SpiralManager Instance { get; private set; }

		private List<NomaiWallText> _nomaiWallTexts;

		public void Awake()
		{
			Instance = this;
			QSBSceneManager.OnUniverseSceneLoaded += OnSceneLoaded;
		}

		public void OnDestroy() => QSBSceneManager.OnUniverseSceneLoaded -= OnSceneLoaded;

		private void OnSceneLoaded(OWScene scene) => _nomaiWallTexts = QSBWorldSync.Init<QSBWallText, NomaiWallText>();

		public int GetId(NomaiWallText obj) => _nomaiWallTexts.IndexOf(obj);
	}
}
