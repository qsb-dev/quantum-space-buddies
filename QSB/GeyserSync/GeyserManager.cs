using QSB.Patches;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.GeyserSync
{
	public class GeyserManager : MonoBehaviour
	{
		public void Awake()
		{
			QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
			QSBPatchManager.OnPatchType += OnPatchType;
		}

		public void OnDestroy()
		{
			QSBSceneManager.OnSceneLoaded -= OnSceneLoaded;
			QSBPatchManager.OnPatchType -= OnPatchType;
		}

		private void OnSceneLoaded(OWScene scene, bool isInUniverse)
			=> QSBWorldSync.Init<QSBGeyser, GeyserController>();

		public void OnPatchType(QSBPatchTypes type)
		{
			if (type != QSBPatchTypes.OnNonServerClientConnect)
			{
				return;
			}
			QSBCore.Helper.HarmonyHelper.EmptyMethod<GeyserController>("Update");
		}
	}
}