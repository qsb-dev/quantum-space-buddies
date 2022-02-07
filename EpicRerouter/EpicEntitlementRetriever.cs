using Epic.OnlineServices;
using Epic.OnlineServices.Ecom;
using UnityEngine;
using static EntitlementsManager;

namespace EpicRerouter
{
	/// <summary>
	/// runs on process side
	/// </summary>
	public static class EpicEntitlementRetriever
	{
		public const string EOS_DLC_OFFER_ID = "379b9688bb1048f3a56d52b13800f4df";
		public const string EOS_DLC_ITEM_ID = "49a9ac61fe464cbf8c8c73f46b3f1133";

		private static EcomInterface _ecomInterface;
		private static OwnershipStatus _epicDlcOwnershipStatus;
		private static bool _epicResultReceived;

		public static void Init()
		{
			EpicPlatformManager.onAuthSuccess += EOSQueryOwnership;
		}

		public static void Uninit()
		{
			EpicPlatformManager.onAuthSuccess -= EOSQueryOwnership;
		}

		public static AsyncOwnershipStatus GetOwnershipStatus()
		{
			if (!_epicResultReceived)
			{
				return AsyncOwnershipStatus.NotReady;
			}

			if (_epicDlcOwnershipStatus != OwnershipStatus.Owned)
			{
				return AsyncOwnershipStatus.NotOwned;
			}

			return AsyncOwnershipStatus.Owned;
		}

		public static void EOSQueryOwnership()
		{
			Debug.Log("[EOS] querying DLC ownership");
			_ecomInterface = EpicPlatformManager.PlatformInterface.GetEcomInterface();
			var queryOwnershipOptions = new QueryOwnershipOptions
			{
				LocalUserId = EpicPlatformManager.LocalUserId,
				CatalogItemIds = new[] { "49a9ac61fe464cbf8c8c73f46b3f1133" }
			};
			_ecomInterface.QueryOwnership(queryOwnershipOptions, null, OnEOSQueryOwnershipComplete);
		}

		private static void OnEOSQueryOwnershipComplete(QueryOwnershipCallbackInfo data)
		{
			if (data.ResultCode == Result.Success)
			{
				_epicDlcOwnershipStatus = data.ItemOwnership[0].OwnershipStatus;
				_epicResultReceived = true;
				Debug.Log($"[EOS] Query DLC ownership complete: {_epicDlcOwnershipStatus}");
			}
		}
	}
}
