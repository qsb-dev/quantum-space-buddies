using QSB.GeyserSync.WorldObjects;
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
			PatchManager.OnPatchType += OnPatchType;
		}

		public void OnDestroy()
		{
			QSBSceneManager.OnSceneLoaded -= OnSceneLoaded;
			PatchManager.OnPatchType -= OnPatchType;
		}

		private void OnSceneLoaded(OWScene scene, bool isInUniverse)
			=> WorldObjectManager.Init<QSBGeyser, GeyserController>();

		public void OnPatchType(PatchType type)
		{
			if (type != PatchType.OnNonServerClientConnect)
			{
				return;
			}
			QSBCore.Helper.HarmonyHelper.EmptyMethod<GeyserController>("Update");
		}
	}
}