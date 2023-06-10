using Epic.OnlineServices;
using Epic.OnlineServices.Ecom;

namespace SteamRerouter.ExeSide;

public static class EpicEntitlementRetriever
{
	private const string _eosDlcItemID = "49a9ac61fe464cbf8c8c73f46b3f1133";

	private static EcomInterface _ecomInterface;
	private static OwnershipStatus _epicDlcOwnershipStatus;
	private static bool _epicResultReceived;

	public static void Init() =>
		EpicPlatformManager.OnAuthSuccess += EOSQueryOwnership;

	public static void Uninit() =>
		EpicPlatformManager.OnAuthSuccess -= EOSQueryOwnership;

	public static EntitlementsManager.AsyncOwnershipStatus GetOwnershipStatus()
	{
		if (!_epicResultReceived)
		{
			return EntitlementsManager.AsyncOwnershipStatus.NotReady;
		}

		return _epicDlcOwnershipStatus == OwnershipStatus.Owned ?
			EntitlementsManager.AsyncOwnershipStatus.Owned : EntitlementsManager.AsyncOwnershipStatus.NotOwned;
	}

	private static void EOSQueryOwnership()
	{
		Program.Log("[EOS] querying DLC ownership");
		_ecomInterface = EpicPlatformManager.PlatformInterface.GetEcomInterface();
		var queryOwnershipOptions = new QueryOwnershipOptions
		{
			LocalUserId = EpicPlatformManager.LocalUserId,
			CatalogItemIds = new[] { _eosDlcItemID }
		};
		_ecomInterface.QueryOwnership(queryOwnershipOptions, null, OnEOSQueryOwnershipComplete);
	}

	private static void OnEOSQueryOwnershipComplete(QueryOwnershipCallbackInfo data)
	{
		if (data.ResultCode == Result.Success)
		{
			_epicDlcOwnershipStatus = data.ItemOwnership[0].OwnershipStatus;
			_epicResultReceived = true;
			Program.Log($"[EOS] Query DLC ownership complete: {_epicDlcOwnershipStatus}");
		}
	}
}