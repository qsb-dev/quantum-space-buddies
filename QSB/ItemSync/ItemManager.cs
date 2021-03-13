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
			QSBWorldSync.Init<QSBWarpCoreItem, WarpCoreItem>();
			QSBWorldSync.Init<QSBWarpCoreSocket, WarpCoreSocket>();
			QSBWorldSync.Init<QSBNomaiConversationStone, NomaiConversationStone>();
			QSBWorldSync.Init<QSBNomaiConversationStoneSocket, NomaiConversationStoneSocket>();
			foreach (var streaming in Resources.FindObjectsOfTypeAll<NomaiRemoteCameraStreaming>())
			{
				streaming.gameObject.AddComponent<CustomNomaiRemoteCameraStreaming>();
				streaming.enabled = false;
			}
			foreach (var camera in Resources.FindObjectsOfTypeAll<NomaiRemoteCamera>())
			{
				camera.gameObject.AddComponent<CustomNomaiRemoteCamera>();
				camera.enabled = false;
			}
			foreach (var platform in Resources.FindObjectsOfTypeAll<NomaiRemoteCameraPlatform>())
			{
				platform.gameObject.AddComponent<CustomNomaiRemoteCameraPlatform>();
				platform.enabled = false;
			}
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
			else if (unityObject.GetType() == typeof(SharedStone))
			{
				worldObj = QSBWorldSync.GetWorldFromUnity<QSBSharedStone, SharedStone>((SharedStone)unityObject);
			}
			else if (unityObject.GetType() == typeof(WarpCoreItem))
			{
				worldObj = QSBWorldSync.GetWorldFromUnity<QSBWarpCoreItem, WarpCoreItem>((WarpCoreItem)unityObject);
			}
			else if (unityObject.GetType() == typeof(NomaiConversationStone))
			{
				worldObj = QSBWorldSync.GetWorldFromUnity<QSBNomaiConversationStone, NomaiConversationStone>((NomaiConversationStone)unityObject);
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
			else if (unityObject.GetType() == typeof(SharedStoneSocket))
			{
				worldObj = QSBWorldSync.GetWorldFromUnity<QSBSharedStoneSocket, SharedStoneSocket>((SharedStoneSocket)unityObject);
			}
			else if (unityObject.GetType() == typeof(WarpCoreSocket))
			{
				worldObj = QSBWorldSync.GetWorldFromUnity<QSBWarpCoreSocket, WarpCoreSocket>((WarpCoreSocket)unityObject);
			}
			else if (unityObject.GetType() == typeof(NomaiConversationStone))
			{
				worldObj = QSBWorldSync.GetWorldFromUnity<QSBNomaiConversationStoneSocket, NomaiConversationStoneSocket>((NomaiConversationStoneSocket)unityObject);
			}
			else
			{
				DebugLog.ToConsole($"Warning - couldn't work out type of OWItemSocket {unityObject.name}.", MessageType.Warning);
			}
			return worldObj;
		}
	}
}
