using OWML.Common;
using QSB.ItemSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.ItemSync
{
	internal class ItemManager : MonoBehaviour
	{
		public static ItemManager Instance { get; private set; }

		public void Awake()
		{
			Instance = this;
			QSBSceneManager.OnUniverseSceneLoaded += RebuildItems;
		}

		public void OnDestroy() => QSBSceneManager.OnUniverseSceneLoaded -= RebuildItems;

		public void RebuildItems(OWScene scene)
		{
			DebugLog.DebugWrite("Rebuilding OWItems...", MessageType.Warning);
			QSBWorldSync.Init<QSBScrollItem, ScrollItem>();
			QSBWorldSync.Init<QSBScrollSocket, ScrollSocket>();
		}
	}
}
