using QSB.Patches;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.GeyserSync
{
	public class GeyserManager : MonoBehaviour
	{
		public static GeyserManager Instance { get; private set; }

		private void Awake()
		{
			Instance = this;
			QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
			QSBPatchManager.OnPatchType += OnPatchType;
		}

		private void OnDestroy()
		{
			QSBSceneManager.OnSceneLoaded -= OnSceneLoaded;
			QSBPatchManager.OnPatchType -= OnPatchType;
		}

		private void OnSceneLoaded(OWScene scene, bool isInUniverse)
		{
			var geyserControllers = Resources.FindObjectsOfTypeAll<GeyserController>();
			for (var id = 0; id < geyserControllers.Length; id++)
			{
				var qsbGeyser = WorldRegistry.GetObject<QSBGeyser>(id) ?? new QSBGeyser();
				qsbGeyser.Init(geyserControllers[id], id);
				WorldRegistry.AddObject(qsbGeyser);
			}
		}

		public void OnPatchType(QSBPatchTypes type)
		{
			if (type != QSBPatchTypes.OnNonServerClientConnect)
			{
				return;
			}
			QSB.Helper.HarmonyHelper.EmptyMethod<GeyserController>("Update");
		}
	}
}