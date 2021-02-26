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
			QSBWorldSync.Init<QSBSharedStone, SharedStone>();
			QSBWorldSync.Init<QSBSharedStoneSocket, SharedStoneSocket>();
		}

		public static IQSBOWItem GetObject(OWItem unityObject)
		{
			if (unityObject == null)
			{
				return default;
			}
			IQSBOWItem worldObj = null;
			if (unityObject.GetType() == typeof(ScrollItem))
			{
				worldObj = QSBWorldSync.GetWorldFromUnity<QSBScrollItem, ScrollItem>((ScrollItem)unityObject);
			}
			if (unityObject.GetType() == typeof(SharedStone))
			{
				worldObj = QSBWorldSync.GetWorldFromUnity<QSBSharedStone, SharedStone>((SharedStone)unityObject);
			}
			else
			{
				DebugLog.ToConsole($"Warning - couldn't work out type of OWItem {unityObject.name}.", MessageType.Warning);
			}
			return worldObj;
		}

		public static IQSBOWItemSocket GetObject(OWItemSocket unityObject)
		{
			if (unityObject == null)
			{
				return default;
			}
			IQSBOWItemSocket worldObj = null;
			if (unityObject.GetType() == typeof(ScrollSocket))
			{
				worldObj = QSBWorldSync.GetWorldFromUnity<QSBScrollSocket, ScrollSocket>((ScrollSocket)unityObject);
			}
			if (unityObject.GetType() == typeof(SharedStoneSocket))
			{
				worldObj = QSBWorldSync.GetWorldFromUnity<QSBSharedStoneSocket, SharedStoneSocket>((SharedStoneSocket)unityObject);
			}
			else
			{
				DebugLog.ToConsole($"Warning - couldn't work out type of OWItemSocket {unityObject.name}.", MessageType.Warning);
			}
			return worldObj;
		}
	}
}
