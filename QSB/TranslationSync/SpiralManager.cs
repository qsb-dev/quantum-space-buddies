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
		private List<NomaiComputer> _nomaiComputers;
		private List<NomaiVesselComputer> _nomaiVesselComputers;

		public void Awake()
		{
			Instance = this;
			QSBSceneManager.OnUniverseSceneLoaded += OnSceneLoaded;
		}

		public void OnDestroy() => QSBSceneManager.OnUniverseSceneLoaded -= OnSceneLoaded;

		private void OnSceneLoaded(OWScene scene)
		{
			_nomaiWallTexts = WorldObjectManager.Init<QSBWallText, NomaiWallText>();
			_nomaiComputers = WorldObjectManager.Init<QSBComputer, NomaiComputer>();
			_nomaiVesselComputers = WorldObjectManager.Init<QSBVesselComputer, NomaiVesselComputer>();
		}

		public int GetId(NomaiWallText obj) => _nomaiWallTexts.IndexOf(obj);
		public int GetId(NomaiComputer obj) => _nomaiComputers.IndexOf(obj);
		public int GetId(NomaiVesselComputer obj) => _nomaiVesselComputers.IndexOf(obj);
	}
}
