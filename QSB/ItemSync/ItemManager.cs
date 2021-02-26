using OWML.Common;
using QSB.ItemSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.ItemSync
{
	internal class ItemManager : MonoBehaviour
	{
		public static ItemManager Instance { get; private set; }

		private List<ScrollItem> _oldScrollList = new List<ScrollItem>();

		public void Awake()
		{
			Instance = this;
			QSBSceneManager.OnUniverseSceneLoaded += RebuildItems;
		}

		public void OnDestroy() => QSBSceneManager.OnUniverseSceneLoaded -= RebuildItems;

		public void RebuildItems(OWScene scene)
		{
			DebugLog.DebugWrite("Rebuilding OWItems...", MessageType.Warning);
			_oldScrollList = QSBWorldSync.Init<QSBScrollItem, ScrollItem>();
			QSBWorldSync.Init<QSBScrollSocket, ScrollSocket>();
		}

		public void OnRenderObject()
		{
			if (!QSBCore.HasWokenUp || !QSBCore.DebugMode || !QSBCore.ShowLinesInDebug)
			{
				return;
			}

			foreach (var item in _oldScrollList)
			{
				Popcron.Gizmos.Cube(item.transform.position, item.transform.rotation, Vector3.one, Color.blue);
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
			else
			{
				DebugLog.ToConsole($"Warning - couldn't work out type of OWItemSocket {unityObject.name}.", MessageType.Warning);
			}
			return worldObj;
		}
	}
}
